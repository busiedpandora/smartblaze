namespace SmartBlaze.Frontend.Models;

public class ChatbotSettings
{
    public string ChatbotName { get; set; } = "";
    public string ApiKey { get; set; } = "";
    public string ApiHost { get; set; } = "";
    public string ChatbotModel { get; set; } = "";
    public List<string> ChatbotModels { get; set; } = [];
    public float Temperature { get; set; }
    public float MinTemperature { get; set; }
    public float MaxTemperature { get; set; }
}