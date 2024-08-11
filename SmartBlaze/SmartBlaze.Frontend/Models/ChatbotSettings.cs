namespace SmartBlaze.Frontend.Models;

public class ChatbotSettings
{
    public string ChatbotName { get; set; } = "";
    public string ApiKey { get; set; } = "";
    public string ApiHost { get; set; } = "";
    public string TextGenerationChatbotModel { get; set; } = "";
    public string ImageGenerationChatbotModel { get; set; } = "";
    public List<string> TextGenerationChatbotModels { get; set; } = [];
    public List<string> ImageGenerationChatbotModels { get; set; } = [];
    public float Temperature { get; set; }
    public float MinTemperature { get; set; }
    public float MaxTemperature { get; set; }
}