namespace SmartBlaze.Frontend.Models;

public class ChatbotDefaultSettings
{
    public string ChatbotName { get; set; } = "";
    public string ApiKey { get; set; } = "";
    public string ApiHost { get; set; } = "";
    public string TextGenerationChatbotModel { get; set; } = "";
    public string ImageGenerationChatbotModel { get; set; } = "";
    public float Temperature { get; set; }
}