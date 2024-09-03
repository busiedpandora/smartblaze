using System.Text.Json.Serialization;

namespace SmartBlaze.Backend.Dtos;

public class ChatSessionEditDto
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }
    
    [JsonPropertyName("chatSessionConfiguration")]
    public ChatSessionConfigurationDto? ChatSessionConfigurationDto { get; set; }
}