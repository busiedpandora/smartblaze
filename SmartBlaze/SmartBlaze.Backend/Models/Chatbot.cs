using SmartBlaze.Backend.Dtos;

namespace SmartBlaze.Backend.Models;

public abstract class Chatbot
{
    private string _name;

    private List<TextGenerationChatbotModel> _textGenerationChatbotModels;
    private List<ImageGenerationChatbotModel> _imageGenerationChatbotModels;

    private bool _supportImageGeneration;

    
    protected Chatbot(string name, 
        List<TextGenerationChatbotModel> textGenerationChatbotModels, List<ImageGenerationChatbotModel> imageGenerationChatbotModels,
        bool supportImageGeneration)
    {
        _name = name;
        _textGenerationChatbotModels = textGenerationChatbotModels;
        _imageGenerationChatbotModels = imageGenerationChatbotModels;
        _supportImageGeneration = supportImageGeneration;
    }

    public string Name => _name;

    public List<TextGenerationChatbotModel> TextGenerationChatbotModels => _textGenerationChatbotModels;

    public List<ImageGenerationChatbotModel> ImageGenerationChatbotModels => _imageGenerationChatbotModels;

    public bool SupportImageGeneration => _supportImageGeneration;

    public abstract Task<AssistantMessageInfoDto> GenerateText(TextGenerationRequestData textGenerationRequestData, HttpClient httpClient);
    
    public abstract IAsyncEnumerable<string> GenerateTextStreamEnabled(TextGenerationRequestData textGenerationRequestData, 
        HttpClient httpClient);

    public abstract Task<AssistantMessageInfoDto> GenerateImage(ImageGenerationRequestData imageGenerationRequestData, HttpClient httpClient);

    public abstract Task<AssistantMessageInfoDto> EntitleChatSession(TextGenerationRequestData textGenerationRequestData, HttpClient httpClient);
    
    public abstract ChatbotDefaultConfigurationDto GetDefaultConfiguration();
}