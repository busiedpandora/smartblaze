using System.Text.Json.Serialization;

namespace SmartBlaze.Frontend.Dtos;

public class ChatSessionInfoDto
{
    [JsonPropertyName("messages")]
    public List<MessageDto>? Messages { get; set; }
    
    [JsonPropertyName("lastUserMessage")]
    public MessageDto? LastUserMessage { get; set; }
    
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
    
    [JsonPropertyName("temperature")]
    public float Temperature { get; set; }
    
    [JsonPropertyName("imageSize")]
    public string? ImageSize { get; set; }
    
    [JsonPropertyName("imagesToGenerate")]
    public int ImagesToGenerate { get; set; }
    
    public int TextStreamDelay { get; set; }
}