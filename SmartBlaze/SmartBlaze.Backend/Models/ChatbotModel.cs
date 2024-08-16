namespace SmartBlaze.Backend.Models;

public class ChatbotModel
{
    private readonly string _name;
    private readonly bool _acceptBase64ImageInput;
    private readonly bool _acceptUrlImageInput;
    private readonly bool _acceptTextFileInput;


    public ChatbotModel(string name, 
        bool acceptBase64ImageInput, bool acceptUrlImageInput, bool acceptTextFileInput)
    {
        _name = name;
        _acceptBase64ImageInput = acceptBase64ImageInput;
        _acceptUrlImageInput = acceptUrlImageInput;
        _acceptTextFileInput = acceptTextFileInput;
    }

    public string Name => _name;

    public bool AcceptBase64ImageInput => _acceptBase64ImageInput;

    public bool AcceptUrlImageInput => _acceptUrlImageInput;

    public bool AcceptTextFileInput => _acceptTextFileInput;
}