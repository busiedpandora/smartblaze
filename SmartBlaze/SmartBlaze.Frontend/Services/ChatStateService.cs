using System.Text;
using System.Text.Json;
using SmartBlaze.Frontend.Dtos;

namespace SmartBlaze.Frontend.Services;

public class ChatStateService
{
    private HttpClient _httpClient;
    
    private List<ChatSessionDto>? _chatSessions;
    private ChatSessionDto? _currentChatSession;
    private List<MessageDto>? _currentChatSessionMessages;
    private bool isGeneratingResponse = false;
    private bool isNewChatBeingCreated = false;

    public ChatStateService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("_httpClient");
        
        if (_chatSessions is null)
        {
            LoadChats();
        }
    }

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

    public bool IsGeneratingResponse
    {
        get => isGeneratingResponse;
    }

    public bool IsNewChatBeingCreated
    {
        get => isNewChatBeingCreated;
    }

    public event Action? OnChange;
    
    public async Task SelectChat(ChatSessionDto chatSession)
    {
        if (_chatSessions is null)
        {
            return;
        }

        if (_currentChatSession is not null && _currentChatSession.Id == chatSession.Id)
        {
            return;
        }
        
        ChatSessionDto? newSelectedChat = _chatSessions.Find(c => c.Id == chatSession.Id);

        if (newSelectedChat is null)
        {
            return;
        }
        
        var response = await _httpClient.GetAsync($"chat-session/{newSelectedChat.Id}/messages");

        if (!response.IsSuccessStatusCode)
        {
            return;
        }
        
        var responseContent = await response.Content.ReadAsStringAsync();
        
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
        
        NotifyStateChanged();
    }

    public async Task SendUserMessage(string content)
    {
        if (_chatSessions is null || _currentChatSession is null || _currentChatSessionMessages is null)
        {
            return;
        }
        
        if(isGeneratingResponse || isNewChatBeingCreated)
        {
            return;
        }
        
        content = content.Trim();

        if (content == string.Empty)
        {
            return;
        }

        isGeneratingResponse = true;
        
        MessageDto? userMessageDto = new MessageDto();
        userMessageDto.Content = content;
            
        var userMessageResponse = await _httpClient
            .PostAsJsonAsync($"chat-session/{_currentChatSession.Id}/new-user-message", 
                userMessageDto);

        if (!userMessageResponse.IsSuccessStatusCode)
        {
            isGeneratingResponse = false;
            return;
        }
        
        var userMessageResponseContent = await userMessageResponse.Content.ReadAsStringAsync();
        userMessageDto = JsonSerializer.Deserialize<MessageDto>(userMessageResponseContent);

        if (userMessageDto is null || !IsMessageValid(userMessageDto))
        {
            isGeneratingResponse = false;
            return;
        }
        
        _currentChatSessionMessages.Add(userMessageDto);
        
        NotifyStateChanged();

        bool textStreaming = true;
        
        if (textStreaming)
        {
            var assistantEmptyMessageResponse = await 
            _httpClient.PostAsJsonAsync(
                $"chat-session/{_currentChatSession.Id}/new-assistant-empty-message", 
                _currentChatSessionMessages);

            if (!assistantEmptyMessageResponse.IsSuccessStatusCode)
            {
                isGeneratingResponse = false;
                return;
            }
            
            var assistantEmptyMessageResponseContent = await assistantEmptyMessageResponse.Content.ReadAsStringAsync();
            var assistantEmptyMessageDto = JsonSerializer.Deserialize<MessageDto>(assistantEmptyMessageResponseContent);

            if (assistantEmptyMessageDto is null || !IsMessageValid(assistantEmptyMessageDto))
            {
                isGeneratingResponse = false;
                return;
            }
            
            _currentChatSessionMessages.Add(assistantEmptyMessageDto);
            
            using var assistantStreamMessageRequest = new HttpRequestMessage(HttpMethod.Post, 
                $"chat-session/{_currentChatSession.Id}/generate-assistant-stream-message");
            assistantStreamMessageRequest.Content = JsonContent.Create(_currentChatSessionMessages);
        
            using var assistantStreamMessageResponse = await _httpClient.SendAsync(assistantStreamMessageRequest, 
                HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
            if (!assistantStreamMessageResponse.IsSuccessStatusCode)
            {
                isGeneratingResponse = false;
                return;
            }
            
            var assistantStreamMessageResponseContent = await assistantStreamMessageResponse.Content.ReadAsStreamAsync()
                .ConfigureAwait(false);

            IAsyncEnumerable<string?> messageChunks =
                JsonSerializer.DeserializeAsyncEnumerable<string>(assistantStreamMessageResponseContent);
            
            isGeneratingResponse = false;
            NotifyStateChanged();

            await foreach (var messageChunk in messageChunks)
            {
                if (messageChunk != string.Empty)
                {
                    assistantEmptyMessageDto.Content += messageChunk;
                    NotifyStateChanged();

                    await Task.Delay(100);
                }
            }
        }
        else
        {
            var assistantMessageResponse = await 
                _httpClient.PostAsJsonAsync(
                    $"chat-session/{_currentChatSession.Id}/new-assistant-message", 
                    _currentChatSessionMessages);

            if (!assistantMessageResponse.IsSuccessStatusCode)
            {
                isGeneratingResponse = false;
                return;
            }
        
            var assistantMessageResponseContent = await assistantMessageResponse.Content.ReadAsStringAsync();
            MessageDto? assistantMessageDto = JsonSerializer.Deserialize<MessageDto>(assistantMessageResponseContent);
        
            if (assistantMessageDto is null || !IsMessageValid(assistantMessageDto))
            {
                isGeneratingResponse = false;
                return;
            }
        
            _currentChatSessionMessages.Add(assistantMessageDto);

            isGeneratingResponse = false;
        
            NotifyStateChanged();
        }
    }

    public async Task CreateNewChatSession()
    {
        if (isNewChatBeingCreated)
        {
            return;
        }

        isNewChatBeingCreated = true;
        
        if (_chatSessions is null)
        {
            _chatSessions = new List<ChatSessionDto>();
        }
        
        ChatSessionDto? chatSessionDto = new ChatSessionDto();
        chatSessionDto.Title = "Undefined";
        
        var response = await _httpClient.PostAsJsonAsync("chat-sessions/new", chatSessionDto);

        if (!response.IsSuccessStatusCode)
        {
            isNewChatBeingCreated = false;
            return;
        }
        
        var responseContent = await response.Content.ReadAsStringAsync();
        chatSessionDto = JsonSerializer.Deserialize<ChatSessionDto>(responseContent);

        if (chatSessionDto is null || !IsChatSessionValid(chatSessionDto))
        {
            isNewChatBeingCreated = false;
            return;
        }

        _chatSessions.Insert(0, chatSessionDto);
        await SelectChat(chatSessionDto);
        NotifyStateChanged();

        if (_currentChatSessionMessages is null)
        {
            isNewChatBeingCreated = false;
            return;
        }
        
        MessageDto? systemMessageDto = new MessageDto();
        systemMessageDto.Content = "You are a helpful assistant. You can help me by answering my questions.";
        
        response = await _httpClient.PostAsJsonAsync(
            $"chat-session/{chatSessionDto.Id}/new-system-message", 
            systemMessageDto);

        if (!response.IsSuccessStatusCode)
        {
            isNewChatBeingCreated = false;
            return;
        }
        
        responseContent = await response.Content.ReadAsStringAsync();
        systemMessageDto = JsonSerializer.Deserialize<MessageDto>(responseContent);

        if (systemMessageDto is null || !IsMessageValid(systemMessageDto))
        {
            isNewChatBeingCreated = false;
            return;
        }
        
        _currentChatSessionMessages.Add(systemMessageDto);
        
        isNewChatBeingCreated = false;
        
        NotifyStateChanged();
    }

    private async void LoadChats()
    {
        var response = await _httpClient.GetAsync($"chat-sessions");

        if (!response.IsSuccessStatusCode)
        {
            return;
        }
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var chatSessionsDto = JsonSerializer.Deserialize<List<ChatSessionDto>>(responseContent);

        if (chatSessionsDto is null)
        {
            _chatSessions = new List<ChatSessionDto>();
        }
        else
        {
            _chatSessions = chatSessionsDto;
        }
        
        NotifyStateChanged();
                
        if (_chatSessions is not null && _chatSessions.Count > 0)
        {
            await SelectChat(_chatSessions.ElementAt(0));
        }
        
        NotifyStateChanged();
    }
    
    private void NotifyStateChanged() => OnChange?.Invoke();

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