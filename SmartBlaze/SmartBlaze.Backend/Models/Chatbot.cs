using SmartBlaze.Backend.Dtos;

namespace SmartBlaze.Backend.Models;

public abstract class Chatbot
{
    private string _name;
    private List<string> _textGenerationModels;
    private List<string> _imageGenerationModels;

    
    protected Chatbot(string name, List<string> textGenerationModels, List<string> imageGenerationModels)
    {
        _name = name;
        _textGenerationModels = textGenerationModels;
        _imageGenerationModels = imageGenerationModels;
    }

    public string Name => _name;
    
    public List<string> TextGenerationModels => _textGenerationModels;
    
    public List<string> ImageGenerationModels => _imageGenerationModels;

    public abstract Task<AssistantMessageInfoDto> GenerateText(ChatSessionInfoDto chatSessionInfoDto, HttpClient httpClient);
    
    public abstract IAsyncEnumerable<string> GenerateTextStreamEnabled(ChatSessionInfoDto chatSessionInfoDto, 
        HttpClient httpClient);

    public abstract Task<AssistantMessageInfoDto> GenerateImage(ChatSessionInfoDto chatSessionInfoDto, HttpClient httpClient);
    
    public abstract ChatbotDefaultConfigurationDto GetDefaultConfiguration();
}