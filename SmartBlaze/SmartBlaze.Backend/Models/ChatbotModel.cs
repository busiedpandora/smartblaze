namespace SmartBlaze.Backend.Models;

public class ChatbotModel
{
    private readonly string _name;
    private readonly bool _acceptSystemInstruction;
    private readonly bool _acceptTemperature;
    private readonly float _minTemperature;
    private readonly float _maxTemperature;
    private readonly bool _acceptBase64ImageInput;
    private readonly bool _acceptUrlImageInput;


    public ChatbotModel(string name, bool acceptSystemInstruction, 
        bool acceptTemperature, float minTemperature, float maxTemperature, 
        bool acceptBase64ImageInput, bool acceptUrlImageInput)
    {
        _name = name;
        _acceptSystemInstruction = acceptSystemInstruction;
        _acceptTemperature = acceptTemperature;
        _minTemperature = minTemperature;
        _maxTemperature = maxTemperature;
        _acceptBase64ImageInput = acceptBase64ImageInput;
        _acceptUrlImageInput = acceptUrlImageInput;
    }

    public string Name => _name;

    public bool AcceptSystemInstruction => _acceptSystemInstruction;

    public bool AcceptTemperature => _acceptTemperature;

    public float MinTemperature => _minTemperature;

    public float MaxTemperature => _maxTemperature;

    public bool AcceptBase64ImageInput => _acceptBase64ImageInput;

    public bool AcceptUrlImageInput => _acceptUrlImageInput;
}