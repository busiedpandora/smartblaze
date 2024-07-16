using System.Text.Json.Serialization;

namespace SmartBlaze.Backend.Dtos;

public class ChatSessionConfigurationDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    
    [JsonPropertyName("systemInstruction")]
    public string? SystemInstruction { get; set; }
    
    [JsonPropertyName("textStream")]
    public bool? TextStream { get; set; }
}