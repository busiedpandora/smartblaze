using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SmartBlaze.Backend.Models;

public class Gemini : Chatbot
{
    public Gemini(string name, string apiHost, string apiKey) : base(name, apiHost, apiKey)
    {
    }

    public override async Task<string?> GenerateAssistantMessageContent(ChatSession chatSession, HttpClient httpClient)
    {
        var contents = new List<Content>();

        foreach (var message in chatSession.Messages)
        {
            var part = new Part
            {
                Text = message.Content
            };

            message.Role = message.Role == Role.System ? Role.User : message.Role;
            message.Role = message.Role == Role.Assistant ? "model" : message.Role;
            
            var content = new Content
            {
                Role = message.Role,
                Parts = new List<Part>() {part}
            };
            contents.Add(content);
        }

        var chatRequest = new ChatRequest
        {
            Contents = contents
        };
        
        var chatRequestJson = JsonSerializer.Serialize(chatRequest);
        
        var httpRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri($"{ApiHost}/v1/models/{chatSession.Model}:generateContent?key={ApiKey}"),
            Content = new StringContent(chatRequestJson, Encoding.UTF8, "application/json")
        };
        
        var chatResponseMessage = await httpClient.SendAsync(httpRequest);
        chatResponseMessage.EnsureSuccessStatusCode();
        
        var chatResponseJson = await chatResponseMessage.Content.ReadAsStringAsync();
        
        ChatResponse? chatResponse = JsonSerializer.Deserialize<ChatResponse>(chatResponseJson);

        if (chatResponse is not null && chatResponse.Candidates is not null)
        {
            var candidate = chatResponse.Candidates[0];

            if (candidate.Content is not null && candidate.Content.Parts is not null)
            {
                return candidate.Content.Parts[0].Text;
            }
        }

        return null;
    }

    private class Part
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }
    
    private class Content
    {
        [JsonPropertyName("role")]
        public string? Role { get; set; }
        
        [JsonPropertyName("parts")]
        public List<Part>? Parts { get; set; }
    }
    
    private class ChatRequest
    {
        [JsonPropertyName("contents")]
        public List<Content>? Contents { get; set; }
    }

    private class Candidate
    {
        [JsonPropertyName("content")]
        public Content? Content { get; set; }
        
        [JsonPropertyName("finish_reason")]
        public string? FinishReason { get; set; }
    
        [JsonPropertyName("index")]
        public int? Index { get; set; }
    }

    private class ChatResponse
    {
        [JsonPropertyName("candidates")]
        public List<Candidate>? Candidates { get; set; }
    }
}