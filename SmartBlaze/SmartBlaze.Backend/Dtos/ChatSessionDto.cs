using System.Text.Json.Serialization;
using SmartBlaze.Backend.Models;

namespace SmartBlaze.Backend.Dtos;

public class ChatSessionDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    
    [JsonPropertyName("title")]
    public string? Title { get; set; }
    
    [JsonPropertyName("creationDate")]
    public DateTime? CreationDate { get; set; }
}