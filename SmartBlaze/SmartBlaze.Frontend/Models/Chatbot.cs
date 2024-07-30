namespace SmartBlaze.Frontend.Models;

public class Chatbot
{
    private string _name;
    private List<string> _models;
    private string _apiHost;
    private string _apiKey;
    private string _model;
    private int _textStreamDelay;
    private float _temperature;
    private float _minTemperature;
    private float _maxtemperature;

    
    public Chatbot(string name, List<string> models, float minTemperature, float maxTemperature)
    {
        _name = name;
        _models = models;
        _minTemperature = minTemperature;
        _maxtemperature = maxTemperature;
    }
    
    public string Name => _name;

    public List<string> Models => _models;

    public string ApiHost
    {
        get => _apiHost;
        set => _apiHost = value ?? throw new ArgumentNullException(nameof(value));
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

    public float Temperature
    {
        get => _temperature;
        set => _temperature = value;
    }

    public float MinTemperature => _minTemperature;

    public float Maxtemperature => _maxtemperature;
}