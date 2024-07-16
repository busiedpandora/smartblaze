using System.Text.Json.Serialization;

namespace SmartBlaze.Frontend.Dtos;

public class ChatbotDto
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    
    [JsonPropertyName("models")]
    public List<string>? Models { get; set; }
    
    [JsonPropertyName("model")]
    public string? Model { get; set; }
    
    [JsonPropertyName("apiHost")]
    public string? ApiHost { get; set; }
    
    [JsonPropertyName("apiKey")]
    public string? ApiKey { get; set; }
    
    [JsonPropertyName("selected")]
    public bool? Selected { get; set; }
}