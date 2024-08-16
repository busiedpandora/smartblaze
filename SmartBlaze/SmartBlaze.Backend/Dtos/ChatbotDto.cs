using System.Text.Json.Serialization;

namespace SmartBlaze.Backend.Dtos;

public class ChatbotDto
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }
    
    [JsonPropertyName("textGenerationChatbotModels")]
    public required List<ChatbotModelDto> TextGenerationChatbotModels { get; set; }
    
    [JsonPropertyName("imageGenerationChatbotModels")]
    public required List<ChatbotModelDto> ImageGenerationChatbotModels { get; set; }
    
    [JsonPropertyName("supportImageGeneration")]
    public required bool SupportImageGeneration { get; set; }
}