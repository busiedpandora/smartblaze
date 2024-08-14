using System.Text.Json.Serialization;

namespace SmartBlaze.Backend.Dtos;

public class MessageDto
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }
    
    [JsonPropertyName("role")]
    public string? Role { get; set; }
    
    [JsonPropertyName("creationDate")]
    public DateTime? CreationDate { get; set; }
    
    [JsonPropertyName("medias")]
    public List<MediaDto>? MediaDtos { get; set; }
    
    [JsonPropertyName("chatbotName")]
    public string? ChatbotName { get; set; }
    
    [JsonPropertyName("chatbotModel")]
    public string? ChatbotModel { get; set; }
    
    [JsonPropertyName("status")]
    public string? Status { get; set; }
}