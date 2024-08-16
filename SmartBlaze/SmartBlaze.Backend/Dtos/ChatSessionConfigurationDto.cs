using System.Text.Json.Serialization;

namespace SmartBlaze.Backend.Dtos;

public class ChatSessionConfigurationDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    
    [JsonPropertyName("chatbotName")]
    public string? ChatbotName { get; set; }
    
    [JsonPropertyName("textGenerationChatbotModel")]
    public string? TextGenerationChatbotModel { get; set; }
    
    [JsonPropertyName("imageGenerationChatbotModel")]
    public string? ImageGenerationChatbotModel { get; set; }
    
    [JsonPropertyName("temperature")]
    public float Temperature { get; set; }
    
    [JsonPropertyName("systemInstruction")]
    public string? SystemInstruction { get; set; }
    
    [JsonPropertyName("textStream")]
    public bool TextStream { get; set; }
    
    [JsonPropertyName("imageSize")]
    public string? ImageSize { get; set; }
    
    [JsonPropertyName("imagesToGenerate")]
    public int ImagesToGenerate { get; set; }
}