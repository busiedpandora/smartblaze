using System.Text.Json.Serialization;

namespace SmartBlaze.Frontend.Dtos;

public class ChatbotConfigurationDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    
    [JsonPropertyName("chatbotName")]
    public string? ChatbotName { get; set; }
    
    [JsonPropertyName("chatbotModel")]
    public string? ChatbotModel { get; set; }
    
    [JsonPropertyName("apiHost")]
    public string? ApiHost { get; set; }
    
    [JsonPropertyName("apiKey")]
    public string? ApiKey { get; set; }
    
    [JsonPropertyName("textStreamDelay")]
    public int TextStreamDelay { get; set; }
    
    [JsonPropertyName("selected")]
    public bool Selected { get; set; }
    
    [JsonPropertyName("models")]
    public List<string>? Models { get; set; }
    
    [JsonPropertyName("temperature")]
    public float Temperature { get; set; }
    
    [JsonPropertyName("minTemperature")]
    public float MinTemperature { get; set; }
    
    [JsonPropertyName("maxTemperature")]
    public float MaxTemperature { get; set; }
}