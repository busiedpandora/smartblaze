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

    public abstract Task<string?> GenerateText(ChatSessionInfoDto chatSessionInfoDto, HttpClient httpClient);
    
    public abstract IAsyncEnumerable<string> GenerateTextStreamEnabled(ChatSessionInfoDto chatSessionInfoDto, 
        HttpClient httpClient);

    public abstract ChatbotDefaultConfigurationDto GetDefaultConfiguration();
}