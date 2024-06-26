using System.Text.Json;
using SmartBlaze.Frontend.Dtos;
using SmartBlaze.Frontend.Models;

namespace SmartBlaze.Frontend.Services;

public class ChatStateService
{
    private HttpClient _httpClient;
    
    private List<ChatSession>? _chatSessions;
    private ChatSession? _currentChatSession;

    public ChatStateService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("_httpClient");
        
        if (_chatSessions is null)
        {
            LoadChats();
        }
    }

    public List<ChatSession>? Chats
    {
        get => _chatSessions;
    }

    public ChatSession? CurrentChatSession
    {
        get => _currentChatSession;
    }
    
    public event Action? OnChange;
    
    public async Task SelectChat(ChatSession chatSession)
    {
        if (_chatSessions is null)
        {
            return;
        }

        if (_currentChatSession is not null && _currentChatSession.Id == chatSession.Id)
        {
            return;
        }
        
        ChatSession? newSelectedChat = _chatSessions.Find(c => c.Id == chatSession.Id);

        if (newSelectedChat is null)
        {
            return;
        }
        
        var response = await _httpClient.GetAsync($"chatsessions/{newSelectedChat.Id}/messages");

        if (!response.IsSuccessStatusCode)
        {
            return;
        }
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var messagesDto = JsonSerializer.Deserialize<List<MessageDto>>(responseContent);

        if (messagesDto is not null)
        {
            var messages = new List<Message>();
            foreach (var messageDto in messagesDto)
            {
                if (IsMessageValid(messageDto))
                {
                    messages.Add(new Message(messageDto.Content, messageDto.Role, messageDto.CreationDate.Value));
                }
            }
            
            newSelectedChat.Messages = messages;
        }
        else
        {
            newSelectedChat.Messages = new List<Message>();
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
        content = content.Trim();

        if (content == string.Empty)
        {
            return;
        }

        if (_currentChatSession is null)
        {
            return;
        }
        
        MessageDto? userMessageDto = new MessageDto();
        userMessageDto.Content = content;
            
        var response = await _httpClient
            .PostAsJsonAsync($"chatsessions/{_currentChatSession.Id}/new-user-message", 
                userMessageDto);

        if (!response.IsSuccessStatusCode)
        {
            return;
        }
        
        var responseContent = await response.Content.ReadAsStringAsync();
        userMessageDto = JsonSerializer.Deserialize<MessageDto>(responseContent);

        if (!IsMessageValid(userMessageDto))
        {
            return;
        }
        
        Message userMessage = new Message(userMessageDto.Content, userMessageDto.Role, userMessageDto.CreationDate.Value);
        _currentChatSession.Messages.Add(userMessage);
        NotifyStateChanged();
        
        response = await 
            _httpClient.PostAsJsonAsync(
                $"chatsessions/{_currentChatSession.Id}/new-assistant-message", 
                "");

        if (!response.IsSuccessStatusCode)
        {
            return;
        }
        
        responseContent = await response.Content.ReadAsStringAsync();
        MessageDto? assistantMessageDto = JsonSerializer.Deserialize<MessageDto>(responseContent);
        
        if (!IsMessageValid(assistantMessageDto))
        {
            return;
        }
        
        Message assistantMessage = new Message(assistantMessageDto.Content, assistantMessageDto.Role, 
            assistantMessageDto.CreationDate.Value);
        _currentChatSession.Messages.Add(assistantMessage);
        NotifyStateChanged();
    }

    public async void CreateNewChatSession()
    {
        if (_chatSessions is null)
        {
            _chatSessions = new List<ChatSession>();
        }
        
        ChatSessionDto? chatSessionDto = new ChatSessionDto();
        chatSessionDto.Title = "Undefined";
        
        var response = await _httpClient.PostAsJsonAsync("chatsessions/new", chatSessionDto);

        if (!response.IsSuccessStatusCode)
        {
            return;
        }
        
        var responseContent = await response.Content.ReadAsStringAsync();
        chatSessionDto = JsonSerializer.Deserialize<ChatSessionDto>(responseContent);

        if (!IsChatSessionValid(chatSessionDto))
        {
            return;
        }
        
        ChatSession chatSession = 
            new ChatSession(chatSessionDto.Id.Value, chatSessionDto.Title, chatSessionDto.CreationDate.Value);

        _chatSessions.Insert(0, chatSession);
        await SelectChat(chatSession);
        NotifyStateChanged();
        
        MessageDto? systemMessageDto = new MessageDto();
        systemMessageDto.Content = "You are a helpful assistant. You can help me by answering my questions. " +
                                   "You can also ask me questions.";
        
        response = await _httpClient.PostAsJsonAsync(
            $"chatsessions/{chatSession.Id}/new-system-message", 
            systemMessageDto);

        if (!response.IsSuccessStatusCode)
        {
            return;
        }
        
        responseContent = await response.Content.ReadAsStringAsync();
        systemMessageDto = JsonSerializer.Deserialize<MessageDto>(responseContent);

        if (!IsMessageValid(systemMessageDto))
        {
            return;
        }
        
        Message systemMessage = new Message(systemMessageDto.Content, systemMessageDto.Role, 
            systemMessageDto.CreationDate.Value);
        chatSession.Messages.Add(systemMessage);
        NotifyStateChanged();
    }

    private async void LoadChats()
    {
        _chatSessions = new List<ChatSession>();
        
        var response = await _httpClient.GetAsync($"chatsessions");

        if (!response.IsSuccessStatusCode)
        {
            return;
        }
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var chatSessionsDto = JsonSerializer.Deserialize<List<ChatSessionDto>>(responseContent);

        if (chatSessionsDto is null)
        {
            return;
        }
        
        foreach (var chatSessionDto in chatSessionsDto)
        {
            if (IsChatSessionValid(chatSessionDto))
            {
                _chatSessions.Add(
                    new ChatSession(chatSessionDto.Id.Value, chatSessionDto.Title, 
                        chatSessionDto.CreationDate.Value));
                        
                NotifyStateChanged();
            }
        }
                
        if (_chatSessions is not null && _chatSessions.Count > 0)
        {
            await SelectChat(_chatSessions.ElementAt(0));
        }
        
        NotifyStateChanged();
    }
    
    private void NotifyStateChanged() => OnChange?.Invoke();

    private bool IsChatSessionValid(ChatSessionDto? chatSessionDto)
    {
        return chatSessionDto is not null 
               && chatSessionDto.Id is not null
               && chatSessionDto.Title is not null
               && chatSessionDto.CreationDate is not null;
    }

    private bool IsMessageValid(MessageDto? messageDto)
    {
        return messageDto is not null
               && messageDto.Content is not null
               && messageDto.Role is not null
               && messageDto.CreationDate is not null;
    }
}