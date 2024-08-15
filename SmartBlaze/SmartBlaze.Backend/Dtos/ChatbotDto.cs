namespace SmartBlaze.Backend.Dtos;

public class ChatbotDto
{
    public required string Name { get; set; }
    public required List<ChatbotModelDto> TextGenerationChatbotModels { get; set; }
    public required List<ChatbotModelDto> ImageGenerationChatbotModels { get; set; }
}