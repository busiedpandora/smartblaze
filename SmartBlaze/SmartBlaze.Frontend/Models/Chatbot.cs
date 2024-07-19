namespace SmartBlaze.Frontend.Models;

public class Chatbot
{
    private string _name;
    private List<string> _models;
    private string _apihost;
    private string _apiKey;
    private string _model;
    private int _textStreamDelay;
    
    
    public Chatbot(string name, List<string> models, string apihost, string apiKey, string model, int textStreamDelay)
    {
        _name = name;
        _models = models;
        _apihost = apihost;
        _apiKey = apiKey;
        _model = model;
        _textStreamDelay = textStreamDelay;
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

    public int TextStreamDelay
    {
        get => _textStreamDelay;
        set => _textStreamDelay = value;
    }
}