namespace SmartBlaze.Frontend.Models;

public class ChatSessionDefaultSettings
{
    public string SystemInstruction { get; set; } = "";
    public bool TextStream { get; set; }
    
    public string ImageSize { get; set; } = "";
    
    public int ImagesToGenerate { get; set; }
}