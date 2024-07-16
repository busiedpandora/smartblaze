namespace SmartBlaze.Frontend.Models;

public class Chatbot
{
    private string _name;
    private List<string> _models;
    private string _apihost;
    private string _apiKey;
    private string _model;
    
    
    public Chatbot(string name, List<string> models, string apihost, string apiKey, string model)
    {
        _name = name;
        _models = models;
        _apihost = apihost;
        _apiKey = apiKey;
        _model = model;
    }

    public string Name => _name;

    public List<string> Models => _models;

    public string Apihost
    {
        get => _apihost;
        set => _apihost = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string ApiKey
    {
        get => _apiKey;
        set => _apiKey = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string Model
    {
        get => _model;
        set => _model = value ?? throw new ArgumentNullException(nameof(value));
    }
}