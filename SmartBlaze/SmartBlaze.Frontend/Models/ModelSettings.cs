namespace SmartBlaze.Frontend.Models;

public class ModelSettings
{
    public string ChatbotName { get; set; } = "";
    public string ApiKey { get; set; } = "";
    public string ApiHost { get; set; } = "";
    public string ChatbotModel { get; set; } = "";
    public List<string> ChatbotModels { get; set; } = [];
}