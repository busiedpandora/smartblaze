using System.Text.Json.Serialization;

namespace SmartBlaze.Frontend.Dtos;

public class MediaDto
{
    [JsonPropertyName("data")]
    public string? Data { get; set; }
    
    [JsonPropertyName("contentType")]
    public string? ContentType { get; set; }
}