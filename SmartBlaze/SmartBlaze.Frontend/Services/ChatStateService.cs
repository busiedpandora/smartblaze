using System.Text.Json;
using SmartBlaze.Frontend.Dtos;

namespace SmartBlaze.Frontend.Services;

public class ChatStateService(IHttpClientFactory httpClientFactory) : AbstractService(httpClientFactory)
{
    private List<ChatSessionDto>? _chatSessions;
    private ChatSessionDto? _currentChatSession;
    private List<MessageDto>? _currentChatSessionMessages;

    private bool _areChatSessionsLoadingOnStartup;
    private bool _isNewChatSessionBeingCreated;
    private bool _isChatSessionBeingSelected;
    private bool _isGeneratingResponse;

    
    public List<ChatSessionDto>? ChatSessions
    {
        get => _chatSessions;
    }

    public ChatSessionDto? CurrentChatSession
    {
        get => _currentChatSession;
    }

    public List<MessageDto>? CurrentChatSessionMessages
    {
        get => _currentChatSessionMessages;
    }

    public bool AreChatSessionsLoadingOnStartup
    {
        get => _areChatSessionsLoadingOnStartup;
        set => _areChatSessionsLoadingOnStartup = value;
    }

    public bool IsNewChatSessionBeingCreated
    {
        get => _isNewChatSessionBeingCreated;
    }

    public bool IsChatSessionBeingSelected
    {
        get => _isChatSessionBeingSelected;
    }

    public bool IsGeneratingResponse
    {
        get => _isGeneratingResponse;
    }
    
    public async Task SelectChatSession(ChatSessionDto chatSession)
    {
        if (_chatSessions is null)
        {
            return;
        }
        
        if (!CanUserInteract())
        {
            return;
        }

        if (_currentChatSession is not null && _currentChatSession.Id == chatSession.Id)
        {
            return;
        }

        _isChatSessionBeingSelected = true;
        NotifyRefreshView();
        
        ChatSessionDto? newSelectedChat = _chatSessions.Find(c => c.Id == chatSession.Id);

        if (newSelectedChat is null)
        {
            _isChatSessionBeingSelected = false;
            NotifyNavigateToErrorPage("Error occured while selecting the chat session", 
                $"chat session with id {chatSession.Id} not found");
            return;
        }
        
        var response = await HttpClient.GetAsync($"chat-session/{newSelectedChat.Id}/messages");
        var responseContent = await response.Content.ReadAsStringAsync();
        
        if (!response.IsSuccessStatusCode)
        {
            _isChatSessionBeingSelected = false;
            NotifyNavigateToErrorPage("Error occured while selecting the chat session", 
                responseContent);
            return;
        }
        
        if (responseContent == string.Empty)
        {
            _currentChatSessionMessages = new List<MessageDto>();
            
        }
        else
        {
            List<MessageDto>? messagesDto = JsonSerializer.Deserialize<List<MessageDto>>(responseContent) ?? new List<MessageDto>();
            _currentChatSessionMessages = messagesDto;
        }
        
        newSelectedChat.Selected = true;

        if (_currentChatSession is not null)
        {
            _currentChatSession.Selected = false;
        }

        _currentChatSession = newSelectedChat;
        
        NotifyNavigateToPage("/");

        _isChatSessionBeingSelected = false;
        
        NotifyRefreshView();
    }

    public void DeselectCurrentChatSession()
    {
        if (_chatSessions is null)
        {
            return;
        }
        
        if (_currentChatSession is not null)
        {
            _currentChatSession.Selected = false;

            _currentChatSession = null;
            
            NotifyRefreshView();
        }
    }

    public async Task SendUserMessage(string content, bool textStream)
    {
        if (_chatSessions is null || _currentChatSession is null || _currentChatSessionMessages is null)
        {
            return;
        }
        
        if (!CanUserInteract())
        {
            return;
        }
        
        content = content.Trim();

        if (content == string.Empty)
        {
            return;
        }

        _isGeneratingResponse = true;
        
        MessageDto? userMessageDto = new MessageDto();
        userMessageDto.Content = content;
            
        var userMessageResponse = await HttpClient
            .PostAsJsonAsync($"chat-session/{_currentChatSession.Id}/new-user-message", 
                userMessageDto);
        var userMessageResponseContent = await userMessageResponse.Content.ReadAsStringAsync();

        if (!userMessageResponse.IsSuccessStatusCode)
        {
            _isGeneratingResponse = false;
            NotifyNavigateToErrorPage("Error occured while sending the message", 
                userMessageResponseContent);
            return;
        }
        
        userMessageDto = JsonSerializer.Deserialize<MessageDto>(userMessageResponseContent);

        if (userMessageDto is null || !IsMessageValid(userMessageDto))
        {
            _isGeneratingResponse = false;
            NotifyNavigateToErrorPage("Error occured while sending the message", 
                "the message sent cannot be null and must contain a content and a role");
            return;
        }
        
        _currentChatSessionMessages.Add(userMessageDto);
        
        NotifyRefreshView();
        
        if (textStream)
        {
            var assistantEmptyMessageResponse = await 
                HttpClient.PostAsJsonAsync(
                $"chat-session/{_currentChatSession.Id}/new-assistant-empty-message", 
                _currentChatSessionMessages);
            var assistantEmptyMessageResponseContent = await assistantEmptyMessageResponse.Content.ReadAsStringAsync();
            
            if (!assistantEmptyMessageResponse.IsSuccessStatusCode)
            {
                _isGeneratingResponse = false;
                NotifyNavigateToErrorPage("Error occured while creating a new assistant message", 
                    assistantEmptyMessageResponseContent);
                return;
            }
            
            var assistantEmptyMessageDto = JsonSerializer.Deserialize<MessageDto>(assistantEmptyMessageResponseContent);

            if (assistantEmptyMessageDto is null || !IsMessageValid(assistantEmptyMessageDto))
            {
                _isGeneratingResponse = false;
                NotifyNavigateToErrorPage("Error occured while creating a new assistant message", 
                    "the assistant message cannot be null and must contain a content and a role");
                return;
            }
            
            _currentChatSessionMessages.Add(assistantEmptyMessageDto);
            
            using var assistantStreamMessageRequest = new HttpRequestMessage(HttpMethod.Post, 
                $"chat-session/{_currentChatSession.Id}/generate-assistant-stream-message");
            assistantStreamMessageRequest.Content = JsonContent.Create(_currentChatSessionMessages);
        
            using var assistantStreamMessageResponse = await HttpClient.SendAsync(assistantStreamMessageRequest, 
                HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
            var assistantStreamMessageResponseContent = await assistantStreamMessageResponse.Content.ReadAsStreamAsync()
                .ConfigureAwait(false);
            
            if (!assistantStreamMessageResponse.IsSuccessStatusCode)
            {
                _isGeneratingResponse = false;
                NotifyNavigateToErrorPage("Error occured while generating a new assistant text stream message", 
                    assistantStreamMessageResponseContent.ToString() ?? "");
                return;
            }
            
            IAsyncEnumerable<string?> messageChunks =
                JsonSerializer.DeserializeAsyncEnumerable<string>(assistantStreamMessageResponseContent);
            
            _isGeneratingResponse = false;
            NotifyRefreshView();

            await foreach (var messageChunk in messageChunks)
            {
                if (messageChunk != string.Empty)
                {
                    assistantEmptyMessageDto.Content += messageChunk;
                    NotifyRefreshView();

                    await Task.Delay(100);
                }
            }
        }
        else
        {
            var assistantMessageResponse = await 
                HttpClient.PostAsJsonAsync(
                    $"chat-session/{_currentChatSession.Id}/new-assistant-message", 
                    _currentChatSessionMessages);
            var assistantMessageResponseContent = await assistantMessageResponse.Content.ReadAsStringAsync();
            
            if (!assistantMessageResponse.IsSuccessStatusCode)
            {
                _isGeneratingResponse = false;
                NotifyNavigateToErrorPage("Error occured while creating a new assistant message", 
                    assistantMessageResponseContent);
                return;
            }
        
            MessageDto? assistantMessageDto = JsonSerializer.Deserialize<MessageDto>(assistantMessageResponseContent);
        
            if (assistantMessageDto is null || !IsMessageValid(assistantMessageDto))
            {
                _isGeneratingResponse = false;
                NotifyNavigateToErrorPage("Error occured while creating a new assistant message", 
                    "the assistant message cannot be null and must contain a content and a role");
                return;
            }
        
            _currentChatSessionMessages.Add(assistantMessageDto);

            _isGeneratingResponse = false;
        
            NotifyRefreshView();
        }
    }

    public async Task CreateNewChatSession(string title, string systemInstruction, string chatbotName, string chatbotModel)
    {
        if (!CanUserInteract())
        {
            return;
        }
        
        _isNewChatSessionBeingCreated = true;
        NotifyRefreshView();
        
        if (_chatSessions is null)
        {
            _chatSessions = new List<ChatSessionDto>();
        }
        
        ChatSessionDto? chatSessionDto = new ChatSessionDto();
        chatSessionDto.Title = title;
        chatSessionDto.SystemInstruction = systemInstruction;
        chatSessionDto.ChatbotName = chatbotName;
        chatSessionDto.ChatbotModel = chatbotModel;
        
        var newChatSessionResponse = await HttpClient.PostAsJsonAsync("chat-sessions/new", chatSessionDto);
        var newChatSessionResponseContent = await newChatSessionResponse.Content.ReadAsStringAsync();

        if (!newChatSessionResponse.IsSuccessStatusCode)
        {
            _isNewChatSessionBeingCreated = false;
            NotifyNavigateToErrorPage("Error occured while creating a new chat session", 
                newChatSessionResponseContent);
            return;
        }
        
        chatSessionDto = JsonSerializer.Deserialize<ChatSessionDto>(newChatSessionResponseContent);

        if (chatSessionDto is null || !IsChatSessionValid(chatSessionDto))
        {
            _isNewChatSessionBeingCreated = false;
            NotifyNavigateToErrorPage("Error occured while creating a new chat session", 
                "the chat session cannot be null and must contain a title");
            return;
        }
        
        NotifyNavigateToPage("/");
        
        _chatSessions.Insert(0, chatSessionDto);
        _isNewChatSessionBeingCreated = false;
        await SelectChatSession(chatSessionDto);
        NotifyRefreshView();

        if (_currentChatSession is null || _currentChatSessionMessages is null)
        {
            _isNewChatSessionBeingCreated = false;
            NotifyNavigateToErrorPage("Error occured while creating a new chat session", 
                "the chat session and the list of messages cannot be null");
            return;
        }
        
        NotifyRefreshView();
    }

    public async Task LoadChats()
    {
        if (_chatSessions is not null)
        {
            return;
        }

        _areChatSessionsLoadingOnStartup = true;
        
        var response = await HttpClient.GetAsync($"chat-sessions");
        var responseContent = await response.Content.ReadAsStringAsync();
        
        if (!response.IsSuccessStatusCode)
        {
            _areChatSessionsLoadingOnStartup = false;
            NotifyNavigateToErrorPage("Error occured while loading the chat sessions", 
                responseContent);
            return;
        }
        
        var chatSessionsDto = JsonSerializer.Deserialize<List<ChatSessionDto>>(responseContent) 
                              ?? new List<ChatSessionDto>();
        
         _chatSessions = chatSessionsDto;

         if (_chatSessions.Count > 0)
         {
             NotifyNavigateToPage("/");
         }
         else
         {
             NotifyNavigateToPage("/welcome");
         }
        
        _areChatSessionsLoadingOnStartup = false;
        
        NotifyRefreshView();
                
        if (_chatSessions is not null && _chatSessions.Count > 0)
        {
            await SelectChatSession(_chatSessions.ElementAt(0));
        }
        
        NotifyRefreshView();
    }
    
    public bool CanUserInteract()
    {
        return !_areChatSessionsLoadingOnStartup 
               && !_isNewChatSessionBeingCreated 
               && !_isChatSessionBeingSelected 
               && !_isGeneratingResponse;
    }

    private bool IsChatSessionValid(ChatSessionDto chatSessionDto)
    {
        return chatSessionDto.Id is not null
               && chatSessionDto.Title is not null
               && chatSessionDto.CreationDate is not null;
    }

    private bool IsMessageValid(MessageDto messageDto)
    {
        return messageDto.Content is not null
               && messageDto.Role is not null
               && messageDto.CreationDate is not null;
    }
}