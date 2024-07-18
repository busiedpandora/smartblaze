using System.Text.Json.Serialization;

namespace SmartBlaze.Backend.Dtos;

public class UserImageDto
{
    [JsonPropertyName("content")]
    public string? Content { get; set; }
    
    [JsonPropertyName("type")]
    public string? Type { get; set; }
    
    [JsonPropertyName("contentType")]
    public string? ContentType { get; set; }
}