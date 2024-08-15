using System.Text.Json.Serialization;

namespace SmartBlaze.Frontend.Dtos;

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
    
    /*public bool SupportBase64InputImageFormat { get; set; }
    
    public bool SupportUrlInputImageFormat { get; set; }
    
    public bool SupportImageGeneration { get; set; }*/
}