using SmartBlaze.Backend.Dtos;

namespace SmartBlaze.Backend.Models;

public abstract class Chatbot
{
    private string _name;
    private List<string> _models;


    protected Chatbot(string name, List<string> models)
    {
        _name = name;
        _models = models;
    }

    public string Name => _name;
    
    public List<string> Models => _models;

    public abstract Task<string?> GenerateText(ChatSessionDto chatSessionDto, 
        List<MessageDto> messageDtos, string apiHost, string apiKey, HttpClient httpClient);
    
    public abstract IAsyncEnumerable<string> GenerateTextStreamEnabled(ChatSessionDto chatSessionDto, 
        List<MessageDto> messageDtos, string apiHost, string apiKey, HttpClient httpClient);

    public abstract ChatbotConfigurationDto GetDefaultConfiguration();
}