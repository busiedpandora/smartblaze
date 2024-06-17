using System.Text;
using System.Text.Json;
using SmartBlaze.Backend.Dtos;
using SmartBlaze.Backend.Models;

namespace SmartBlaze.Backend.Services;

public class ChatSessionService
{
    private List<ChatSession> _chatSessions;

    private HttpClient _httpClient;

    public ChatSessionService()
    {
        //_chatSessions = new List<ChatSession>();
        
        ChatSession cs1 = new ChatSession(1L, "chat1");
        cs1.Messages.Add(new Message("Hello. I'm here to assist you!", "system", DateTime.Now));
        cs1.Messages.Add(new Message("What is the capital of Germany?", "user", DateTime.Now));
        cs1.Messages.Add(new Message("The capital of Germany is Berlin", "assistant", DateTime.Now));
        
        ChatSession cs2 = new ChatSession(2L, "chat2");
        cs2.Messages.Add(new Message("Hello. I'm here to assist you!", "system", DateTime.Now));
        cs2.Messages.Add(new Message("What time is it?", "user", DateTime.Now));
        cs2.Messages.Add(new Message("It is 3.00 pm.", "assistant", DateTime.Now));
        
        _chatSessions = new List<ChatSession>() {cs1, cs2};

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
}