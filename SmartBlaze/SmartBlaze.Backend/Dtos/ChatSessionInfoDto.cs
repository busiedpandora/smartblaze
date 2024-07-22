using System.Text.Json.Serialization;

namespace SmartBlaze.Backend.Dtos;

public class ChatSessionInfoDto
{
    [JsonPropertyName("messages")]
    public List<MessageDto>? Messages { get; set; }
    
    [JsonPropertyName("chatbotName")]
    public string? ChatbotName { get; set; }
    
    [JsonPropertyName("chatbotModel")]
    public string? ChatbotModel { get; set; }
    
    [JsonPropertyName("apiHost")]
    public string? ApiHost { get; set; }
    
    [JsonPropertyName("apiKey")]
    public string? ApiKey { get; set; }
    
    [JsonPropertyName("systemInstruction")]
    public string? SystemInstruction { get; set; }
}