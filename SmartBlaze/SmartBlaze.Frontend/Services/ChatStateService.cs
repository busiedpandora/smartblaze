using System.Text.Json;
using SmartBlaze.Frontend.Dtos;
using SmartBlaze.Frontend.Models;

namespace SmartBlaze.Frontend.Services;

public class ChatStateService
{
    private HttpClient _httpClient;
    
    private List<ChatSession>? _chatSessions;
    private ChatSession? _currentChatSession;

    public ChatStateService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        
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
        
        if (_currentChatSession is not null)
        {
            ChatSession? selectedChat = _chatSessions.Find(c => c.Id == _currentChatSession.Id);

            if (selectedChat is not null)
            {
                selectedChat.Selected = false;
            }
        }
        
        ChatSession? newSelectedChat = _chatSessions.Find(c => c.Id == chatSession.Id);

        if (newSelectedChat is not null)
        {
            newSelectedChat.Selected = true;
            
            var response = await _httpClient.GetAsync($"http://localhost:15058/chatsessions/{chatSession.Id}/messages");

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var messagesDto = JsonSerializer.Deserialize<List<MessageDto>>(responseContent);

                if (messagesDto is not null)
                {
                    var messages = messagesDto
                        .Select(m => new Message(m.Content, m.Role, m.CreationDate.Value))
                        .ToList();

                    newSelectedChat.Messages = messages;

                    _currentChatSession = newSelectedChat;

                    NotifyStateChanged();
                }
            }
        }
    }

    public async Task SendUserMessage(string content)
    {
        content = content.Trim();

        if (content == string.Empty)
        {
            return;
        }

        if (_currentChatSession is not null)
        {
            MessageDto? userMessageDto = new MessageDto();
            userMessageDto.Content = content;
            
            var response = await _httpClient
                .PostAsJsonAsync($"http://localhost:15058/chatsessions/{_currentChatSession.Id}/new-user-message", 
                    userMessageDto);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                userMessageDto = JsonSerializer.Deserialize<MessageDto>(responseContent);

                if (userMessageDto is not null
                    && userMessageDto.Content is not null
                    && userMessageDto.Role is not null
                    && userMessageDto.CreationDate is not null)
                {
                    Message userMessage = new Message(userMessageDto.Content, userMessageDto.Role, userMessageDto.CreationDate.Value);
                    _currentChatSession.Messages.Add(userMessage);
                    NotifyStateChanged();

                    response = await 
                        _httpClient.PostAsJsonAsync(
                            $"http://localhost:15058/chatsessions/{_currentChatSession.Id}/new-assistant-message", 
                            "");
                    
                    if (response.IsSuccessStatusCode)
                    {
                        responseContent = await response.Content.ReadAsStringAsync();
                        MessageDto? assistantMessageDto = JsonSerializer.Deserialize<MessageDto>(responseContent);

                        if (assistantMessageDto is not null
                            && assistantMessageDto.Content is not null
                            && assistantMessageDto.Role is not null
                            && assistantMessageDto.CreationDate is not null)
                        {
                            Message assistantMessage = new Message(assistantMessageDto.Content,
                                assistantMessageDto.Role, assistantMessageDto.CreationDate.Value);
                            _currentChatSession.Messages.Add(assistantMessage);
                            NotifyStateChanged();
                        }
                    }
                }
            }
        }
    }

    public async void CreateNewChatSession()
    {
        ChatSessionDto? chatSessionDto = new ChatSessionDto();
        chatSessionDto.Title = "Undefined";

        var response = await _httpClient.PostAsJsonAsync("http://localhost:15058/chatsessions/new", chatSessionDto);

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            chatSessionDto = JsonSerializer.Deserialize<ChatSessionDto>(responseContent);
            
            if (chatSessionDto is not null 
                && chatSessionDto.Id is not null 
                && chatSessionDto.Title is not null 
                && chatSessionDto.CreationDate is not null)
            {
                ChatSession chatSession = 
                    new ChatSession(chatSessionDto.Id.Value, chatSessionDto.Title, chatSessionDto.CreationDate.Value);

                if (_chatSessions is null)
                {
                    _chatSessions = new List<ChatSession>();
                }
                _chatSessions.Insert(0, chatSession);
                await SelectChat(chatSession);
                
                NotifyStateChanged();
            }
        }
    }

    private async void LoadChats()
    {
        _chatSessions = new List<ChatSession>();
        
        var response = await _httpClient.GetAsync($"http://localhost:15058/chatsessions");

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var chatSessionsDto = JsonSerializer.Deserialize<List<ChatSessionDto>>(responseContent);

            if (chatSessionsDto is not null)
            {
                foreach (var chatSessionDto in chatSessionsDto)
                {
                    if (chatSessionDto.Id is not null
                        && chatSessionDto.Title is not null
                        && chatSessionDto.CreationDate is not null)
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
            }
        }
        
        NotifyStateChanged();
    }
    
    private void NotifyStateChanged() => OnChange?.Invoke();
}