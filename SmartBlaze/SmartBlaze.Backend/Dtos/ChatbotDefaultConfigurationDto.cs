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
    
    [JsonPropertyName("textGenerationChatbotModels")]
    public List<string>? TextGenerationChatbotModels { get; set; }
    
    [JsonPropertyName("imageGenerationChatbotModels")]
    public List<string>? ImageGenerationChatbotModels { get; set; }
    
    [JsonPropertyName("textStreamDelay")]
    public int TextStreamDelay { get; set; }
    
    [JsonPropertyName("temperature")]
    public float Temperature { get; set; }
    
    [JsonPropertyName("minTemperature")]
    public float MinTemperature { get; set; }
    
    [JsonPropertyName("maxTemperature")]
    public float MaxTemperature { get; set; }
    
    [JsonPropertyName("supportBase64ImageInputFormat")]
    public bool SupportBase64ImageInputFormat { get; set; }
    
    [JsonPropertyName("supportUrlImageInputFormat")]
    public bool SupportUrlImageInputFormat { get; set; }
    
    [JsonPropertyName("selected")]
    public bool Selected { get; set; }
}