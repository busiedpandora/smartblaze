using System.Text;
using System.Text.Json;
using SmartBlaze.Backend.Dtos;
using SmartBlaze.Backend.Models;

namespace SmartBlaze.Backend.Services;

public class ChatSessionService
{
    private static long _counter = 0L;
    
    private List<ChatSession> _chatSessions;

    private HttpClient _httpClient;

    public ChatSessionService()
    {
        _chatSessions = new List<ChatSession>();
        _httpClient = new HttpClient();
    }
    
    public List<ChatSession> GetAllChatSessions()
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

    public async Task<ChatResponseDto?> GenerateChatResponse(ChatSession chatSession)
    {
        var apiKey = "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";

        var messagesDto = chatSession.Messages.Select(m => new MessageDto(m.Content, m.Role)).ToList();
        
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

        return chatResponseDto;
    }

    public MessageDto? GetMessageFromChatResponse(ChatResponseDto chatResponse)
    {
        if (chatResponse.Choices is not null && chatResponse.Choices.Count > 0)
        {
            MessageDto? messageDto = chatResponse.Choices[0].Message;

            return messageDto;
        }

        return null;
    }

    public ChatSession CreateNewChatSession(string title)
    {
        return new ChatSession(++_counter, title, DateTime.Now);
    }

    public void AddChatSession(ChatSession chatSession)
    {
        _chatSessions.Add(chatSession);
    }
}