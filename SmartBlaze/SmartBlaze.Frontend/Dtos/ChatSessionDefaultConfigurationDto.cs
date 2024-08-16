using System.Text.Json.Serialization;

namespace SmartBlaze.Frontend.Dtos;

public class ChatSessionDefaultConfigurationDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    
    [JsonPropertyName("systemInstruction")]
    public string? SystemInstruction { get; set; }
    
    [JsonPropertyName("textStream")]
    public bool TextStream { get; set; }
    
    [JsonPropertyName("imageSize")]
    public string? ImageSize { get; set; }
    
    [JsonPropertyName("imagesToGenerate")]
    public int ImagesToGenerate { get; set; }
}