using System.Text.Json.Serialization;

namespace SmartBlaze.Backend.Dtos;

public class MessageDto
{
    [JsonPropertyName("content")]
    public string? Content { get; set; }
    
    [JsonPropertyName("role")]
    public string? Role { get; set; }
    
    [JsonPropertyName("creationDate")]
    public DateTime? CreationDate { get; set; }
}