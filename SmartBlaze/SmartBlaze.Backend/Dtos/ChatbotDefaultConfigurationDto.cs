using System.Text.Json.Serialization;

namespace SmartBlaze.Backend.Dtos;

public class ChatbotDefaultConfigurationDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    
    [JsonPropertyName("chatbotName")]
    public string? ChatbotName { get; set; }
    
    [JsonPropertyName("apiHost")]
    public string? ApiHost { get; set; }
    
    [JsonPropertyName("apiKey")]
    public string? ApiKey { get; set; }
    
    [JsonPropertyName("textGenerationChatbotModel")]
    public string? TextGenerationChatbotModel { get; set; }
    
    [JsonPropertyName("imageGenerationChatbotModel")]
    public string? ImageGenerationChatbotModel { get; set; }
    
    [JsonPropertyName("temperature")]
    public float Temperature { get; set; }
    
    [JsonPropertyName("selected")]
    public bool Selected { get; set; }
}