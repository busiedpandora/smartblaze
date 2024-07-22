using System.Text.Json.Serialization;

namespace SmartBlaze.Frontend.Dtos;

public class ChatSessionDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    
    [JsonPropertyName("title")]
    public string? Title { get; set; }
    
    [JsonPropertyName("creationDate")]
    public DateTime? CreationDate { get; set; }
    
    public bool Selected { get; set; }
}