namespace SmartBlaze.Backend.Models;

public class ChatbotModel
{
    private readonly string _name;
    private readonly bool _acceptBase64ImageInput;
    private readonly bool _acceptUrlImageInput;


    public ChatbotModel(string name, 
        bool acceptBase64ImageInput, bool acceptUrlImageInput)
    {
        _name = name;
        _acceptBase64ImageInput = acceptBase64ImageInput;
        _acceptUrlImageInput = acceptUrlImageInput;
    }

    public string Name => _name;

    public bool AcceptBase64ImageInput => _acceptBase64ImageInput;

    public bool AcceptUrlImageInput => _acceptUrlImageInput;
}