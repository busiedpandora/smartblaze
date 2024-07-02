using System.Text.Json.Serialization;
using SmartBlaze.Backend.Models;

namespace SmartBlaze.Backend.Dtos;

public class ChatSessionDto
{
    [JsonPropertyName("id")]
    public long? Id { get; set; }
    
    [JsonPropertyName("title")]
    public string? Title { get; set; }
    
    [JsonPropertyName("creationDate")]
    public DateTime? CreationDate { get; set; }
    
    [JsonPropertyName("messages")]
    public List<MessageDto>? Messages { get; set; }
    
    [JsonPropertyName("chatbotName")]
    public string? ChatbotName { get; set; }
    
    [JsonPropertyName("chatbotModel")]
    public string? ChatbotModel { get; set; }
}