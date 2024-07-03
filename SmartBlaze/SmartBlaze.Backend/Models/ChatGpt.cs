using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using SmartBlaze.Backend.Dtos;

namespace SmartBlaze.Backend.Models;

public class ChatGpt : Chatbot
{
    public ChatGpt(string name, string apiHost, string apiKey) : base(name, apiHost, apiKey)
    {
    }

    public override async Task<string?> GenerateAssistantMessageContent(ChatSessionDto chatSessionDto, 
        List<MessageDto> messageDtos, HttpClient httpClient)
    {
        var messages = messageDtos
            .Select(m => new Message
            {
                Content = m.Content,
                Role = m.Role
            })
            .ToList();

        var chatRequest = new ChatRequest
        {
            Model = chatSessionDto.ChatbotModel,
            Messages = messages
        };

        var chatRequestJson = JsonSerializer.Serialize(chatRequest);
        
        var httpRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri($"{ApiHost}/v1/chat/completions"),
            Headers =
            {
                { "Authorization", $"Bearer {ApiKey}" },
            },
            Content = new StringContent(chatRequestJson, Encoding.UTF8, "application/json")
        };
        
        var chatResponseMessage = await httpClient.SendAsync(httpRequest);
        chatResponseMessage.EnsureSuccessStatusCode();
        
        var chatResponseJson = await chatResponseMessage.Content.ReadAsStringAsync();
        
        ChatResponse? chatResponse = JsonSerializer.Deserialize<ChatResponse>(chatResponseJson);
        
        if (chatResponse is not null && chatResponse.Choices is not null && chatResponse.Choices.Count > 0)
        {
            Message? message = chatResponse.Choices[0].Message;

            if (message is not null)
            {
                return message.Content;
            }
        }

        return null;
    }

    class Message
    {
        [JsonPropertyName("content")]
        public string? Content { get; set; }
    
        [JsonPropertyName("role")]
        public string? Role { get; set; }
    }

    private class ChatRequest
    {
        [JsonPropertyName("model")]
        public string? Model { get; set; }

        [JsonPropertyName("messages")]
        public List<Message>? Messages { get; set; }
    }

    private class ChatResponse
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("choices")]
        public List<Choice>? Choices { get; set; }
    
        [JsonPropertyName("created")]
        public long? Created { get; set; }
    
        [JsonPropertyName("model")]
        public string? Model { get; set; }
    
        [JsonPropertyName("object")]
        public string? Object { get; set; }
    }

    private class Choice
    {
        [JsonPropertyName("finish_reason")]
        public string? FinishReason { get; set; }
    
        [JsonPropertyName("index")]
        public int? Index { get; set; }
    
        [JsonPropertyName("message")]
        public Message? Message { get; set; }
    }
}