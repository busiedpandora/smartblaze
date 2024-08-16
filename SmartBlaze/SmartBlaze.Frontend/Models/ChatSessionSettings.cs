namespace SmartBlaze.Frontend.Models;

public class ChatSessionSettings
{
    public string Title { get; set; } = "";
    
    public string ChatbotName { get; set; } = "";
    
    public string TextGenerationChatbotModel { get; set; } = "";
    
    public string ImageGenerationChatbotModel { get; set; } = "";
    
    public float Temperature { get; set; }
    
    public string SystemInstruction { get; set; } = "";
    
    public bool TextStream { get; set; }
    
    public string ImageSize { get; set; } = "";
    
    public int ImagesToGenerate { get; set; }
}