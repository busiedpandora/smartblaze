using System.Text.Json.Serialization;

namespace SmartBlaze.Frontend.Dtos;

public class ChatSessionInfoDto
{
    [JsonPropertyName("messages")]
    public List<MessageDto>? Messages { get; set; }
    
    [JsonPropertyName("apiHost")]
    public string? ApiHost { get; set; }
    
    [JsonPropertyName("apiKey")]
    public string? ApiKey { get; set; }
    
    public int TextStreamDelay { get; set; }
}