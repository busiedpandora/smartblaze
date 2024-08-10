using System.Text.Json.Serialization;

namespace SmartBlaze.Frontend.Dtos;

public class MediaDto
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    
    [JsonPropertyName("data")]
    public string? Data { get; set; }

    [JsonPropertyName("contentType")] 
    public string ContentType { get; set; } = "";
}