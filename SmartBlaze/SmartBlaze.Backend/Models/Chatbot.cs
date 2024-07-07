using SmartBlaze.Backend.Dtos;

namespace SmartBlaze.Backend.Models;

public abstract class Chatbot
{
    private string _name;
    private string _apiHost;
    private string _apiKey;
    private List<string> _models;


    protected Chatbot(string name, string apiHost, string apiKey)
    {
        _name = name;
        _apiHost = apiHost;
        _apiKey = apiKey;
        _models = new List<string>();
    }

    public string Name => _name;

    public string ApiHost => _apiHost;
    
    public string ApiKey => _apiKey;
    public List<string> Models => _models;

    public abstract Task<string?> GenerateText(ChatSessionDto chatSessionDto, 
        List<MessageDto> messageDtos, HttpClient httpClient);
    
    public abstract IAsyncEnumerable<string> GenerateTextStreamEnabled(ChatSessionDto chatSessionDto, 
        List<MessageDto> messageDtos, HttpClient httpClient);
}