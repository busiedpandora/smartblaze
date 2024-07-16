namespace SmartBlaze.Backend.Dtos;

public class ChatbotConfigurationDto
{
    public string? Id { get; set; }
    public string? ChatbotName { get; set; }
    public string? ChatbotModel { get; set; }
    public string? ApiHost { get; set; }
    public string? ApiKey { get; set; }
    public bool? Selected { get; set; }
}