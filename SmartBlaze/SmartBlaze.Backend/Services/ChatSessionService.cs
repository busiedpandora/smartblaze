using System.Text;
using System.Text.Json;
using SmartBlaze.Backend.Dtos;
using SmartBlaze.Backend.Models;

namespace SmartBlaze.Backend.Services;

public class ChatSessionService
{
    private static long _counter = 2L;
    
    private List<ChatSession> _chatSessions;

    private HttpClient _httpClient;
    
    
    public ChatSessionService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        
        ChatSession c1 = new ChatSession(1L, "chat1", DateTime.Now);
        ChatSession c2 = new ChatSession(2L, "chat2", DateTime.Now);
        _chatSessions = new List<ChatSession>();
        //_chatSessions = new List<ChatSession>(){c1, c2};
    }
    
    public List<ChatSession>? GetAllChatSessions()
    {
        return _chatSessions;
    }

    public ChatSession? GetChatSessionById(long id)
    {
        return _chatSessions.Find(chat => chat.Id.Equals(id));
    }
    
    public void AddNewMessageToChatSession(Message message, ChatSession chatSession)
    {
        chatSession.Messages.Add(message);
    }

    public ChatSession CreateNewChatSession(string title)
    {
        return new ChatSession(++_counter, title, DateTime.Now);
    }

    public void AddChatSession(ChatSession chatSession)
    {
        _chatSessions.Add(chatSession);
    }

    public async Task<string?> GenerateAssistantMessageContentFromChatSession(ChatSession chatSession)
    {
        var apiKey = "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";
        
        var messagesDto = chatSession.Messages
            .Select(m => MessageDto.ToMessageDto(m.Content, m.Role, m.CreationDate))
            .ToList();
        
        var chatRequestDto = new ChatRequestDto("gpt-3.5-turbo",messagesDto);
        var chatRequestJson = JsonSerializer.Serialize(chatRequestDto);
        
        var httpRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri("https://api.openai.com/v1/chat/completions"),
            Headers =
            {
                { "Authorization", $"Bearer {apiKey}" },
            },
            Content = new StringContent(chatRequestJson, Encoding.UTF8, "application/json")
        };
        
        var chatResponse = await _httpClient.SendAsync(httpRequest);
        chatResponse.EnsureSuccessStatusCode();
        
        var chatResponseJson = await chatResponse.Content.ReadAsStringAsync();
        
        ChatResponseDto? chatResponseDto = JsonSerializer.Deserialize<ChatResponseDto>(chatResponseJson);
        
        if (chatResponseDto is not null && chatResponseDto.Choices is not null && chatResponseDto.Choices.Count > 0)
        {
            MessageDto? messageDto = chatResponseDto.Choices[0].Message;

            if (messageDto is not null)
            {
                return messageDto.Content;
            }
        }

        return null;
    }
}