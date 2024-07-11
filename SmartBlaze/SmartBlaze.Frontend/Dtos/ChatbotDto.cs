using System.Text.Json.Serialization;

namespace SmartBlaze.Frontend.Dtos;

public class ChatbotDto
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    
    [JsonPropertyName("models")]
    public List<string>? Models { get; set; }
}