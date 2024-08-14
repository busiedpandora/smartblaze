namespace SmartBlaze.Backend.Models;

public class TextGenerationChatbotModel : ChatbotModel
{
    private readonly bool _acceptTextStream;
    private readonly int _textStreamDelay;
    private readonly bool _acceptImageVision;


    public TextGenerationChatbotModel(string name, bool acceptSystemInstruction, 
        bool acceptTemperature, float minTemperature, float maxTemperature, 
        bool acceptBase64ImageInput, bool acceptUrlImageInput, 
        bool acceptTextStream, int textStreamDelay, bool acceptImageVision) 
        : base(name, acceptSystemInstruction, acceptTemperature, minTemperature, maxTemperature, acceptBase64ImageInput, acceptUrlImageInput)
    {
        _acceptTextStream = acceptTextStream;
        _textStreamDelay = textStreamDelay;
        _acceptImageVision = acceptImageVision;
    }

    public bool AcceptTextStream => _acceptTextStream;

    public int TextStreamDelay => _textStreamDelay;

    public bool AcceptImageVision => _acceptImageVision;
}