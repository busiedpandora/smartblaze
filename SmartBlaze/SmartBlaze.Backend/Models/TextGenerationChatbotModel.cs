namespace SmartBlaze.Backend.Models;

public class TextGenerationChatbotModel : ChatbotModel
{
    private readonly bool _acceptSystemInstruction;
    private readonly bool _acceptTemperature;
    private readonly float _minTemperature;
    private readonly float _maxTemperature;
    private readonly bool _acceptTextStream;
    private readonly int _textStreamDelay;
    private readonly bool _acceptImageVision;


    public TextGenerationChatbotModel(string name, 
        bool acceptBase64ImageInput, bool acceptUrlImageInput, bool acceptTextFileInput,
        bool acceptSystemInstruction, bool acceptTemperature, float minTemperature, float maxTemperature, 
        bool acceptTextStream, int textStreamDelay, bool acceptImageVision) 
        : base(name, acceptBase64ImageInput, acceptUrlImageInput, acceptTextFileInput)
    {
        _acceptSystemInstruction = acceptSystemInstruction;
        _acceptTemperature = acceptTemperature;
        _minTemperature = minTemperature;
        _maxTemperature = maxTemperature;
        _acceptTextStream = acceptTextStream;
        _textStreamDelay = textStreamDelay;
        _acceptImageVision = acceptImageVision;
    }

    
    public bool AcceptSystemInstruction => _acceptSystemInstruction;

    public bool AcceptTemperature => _acceptTemperature;

    public float MinTemperature => _minTemperature;

    public float MaxTemperature => _maxTemperature;
    
    public bool AcceptTextStream => _acceptTextStream;

    public int TextStreamDelay => _textStreamDelay;

    public bool AcceptImageVision => _acceptImageVision;
}