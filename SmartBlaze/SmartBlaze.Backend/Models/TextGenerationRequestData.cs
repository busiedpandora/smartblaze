using SmartBlaze.Backend.Dtos;

namespace SmartBlaze.Backend.Models;

public class TextGenerationRequestData
{
    public required List<MessageDto> Messages { get; set; }
    public required TextGenerationChatbotModel ChatbotModel { get; set; }
    public required string ApiHost { get; set; }
    public required string ApiKey { get; set; }
    public string? SystemInstruction { get; set; }
    public float Temperature { get; set; }
}