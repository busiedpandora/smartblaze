using System.Text.Json.Serialization;

namespace SmartBlaze.Backend.Dtos;

public class UserDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    
    [JsonPropertyName("username")]
    public string? Username { get; set; }
    
    [JsonPropertyName("password")]
    public string? Password { get; set; }
}