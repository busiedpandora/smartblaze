using System.Text.Json.Serialization;

namespace SmartBlaze.Backend.Dtos;

public class MessageDto
{
    public MessageDto(string? content, string? role)
    {
        Content = content;
        Role = role;
    }

    [JsonPropertyName("content")]
    public string? Content { get; set; }
    
    [JsonPropertyName("role")]
    public string? Role { get; set; }
}