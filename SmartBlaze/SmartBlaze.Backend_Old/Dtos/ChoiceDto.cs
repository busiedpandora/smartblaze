using System.Text.Json.Serialization;

namespace SmartBlaze.Backend.Dtos;

public class ChoiceDto
{
    [JsonPropertyName("finish_reason")]
    public string? FinishReason { get; set; }
    
    [JsonPropertyName("index")]
    public int? Index { get; set; }
    
    [JsonPropertyName("message")]
    public MessageDto? Message { get; set; }
}