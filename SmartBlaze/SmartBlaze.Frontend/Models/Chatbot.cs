namespace SmartBlaze.Frontend.Models;

public class Chatbot
{
    private string _name;
    private List<string> _textGenerationModels;
    private List<string> _imageGenerationModels;
    private string _apiHost;
    private string _apiKey;
    private string _textGenerationModel;
    private string _imageGenerationModel;
    private int _textStreamDelay;
    private float _temperature;
    private float _minTemperature;
    private float _maxtemperature;

    
    public Chatbot(string name, List<string> textGenerationModels, List<string> imageGenerationModels, float minTemperature, float maxTemperature)
    {
        _name = name;
        _textGenerationModels = textGenerationModels;
        _imageGenerationModels = imageGenerationModels;
        _minTemperature = minTemperature;
        _maxtemperature = maxTemperature;
    }
    
    public string Name => _name;

    public List<string> TextGenerationModels => _textGenerationModels;

    public List<string> ImageGenerationModels => _imageGenerationModels;

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

    public string TextGenerationModel
    {
        get => _textGenerationModel;
        set => _textGenerationModel = value ?? throw new ArgumentNullException(nameof(value));
    }
    
    public string ImageGenerationModel
    {
        get => _imageGenerationModel;
        set => _imageGenerationModel = value ?? throw new ArgumentNullException(nameof(value));
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