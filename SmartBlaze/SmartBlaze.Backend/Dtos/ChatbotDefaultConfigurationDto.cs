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
    
    [JsonPropertyName("chatbotModel")]
    public string? ChatbotModel { get; set; }
    
    [JsonPropertyName("chatbotModels")]
    public List<string>? ChatbotModels { get; set; }
    
    [JsonPropertyName("textStreamDelay")]
    public int TextStreamDelay { get; set; }
    
    [JsonPropertyName("temperature")]
    public float Temperature { get; set; }
    
    [JsonPropertyName("minTemperature")]
    public float MinTemperature { get; set; }
    
    [JsonPropertyName("maxTemperature")]
    public float MaxTemperature { get; set; }
    
    [JsonPropertyName("selected")]
    public bool Selected { get; set; }
}