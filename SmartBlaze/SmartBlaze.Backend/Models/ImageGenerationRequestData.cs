using SmartBlaze.Backend.Dtos;

namespace SmartBlaze.Backend.Models;

public class ImageGenerationRequestData
{
    public required MessageDto LastUserMessage { get; set; }
    public required ImageGenerationChatbotModel ChatbotModel { get; set; }
    public required string ApiHost { get; set; }
    public required string ApiKey { get; set; }
    public string? SystemInstruction { get; set; }
    public float Temperature { get; set; }
    public string? ImageSize { get; set; }
    public int N { get; set; }
    
}