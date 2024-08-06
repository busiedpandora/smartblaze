namespace SmartBlaze.Frontend.Models;

public class ChatSessionSettings
{
    public string Title { get; set; } = "";
    
    public string ChatbotName { get; set; } = "";
    
    public string ChatbotModel { get; set; } = "";
    
    public List<string> ChatbotModels { get; set; } = [];
    
    public float Temperature { get; set; }
    
    public string SystemInstruction { get; set; } = "";
    
    public bool TextStream { get; set; }
}