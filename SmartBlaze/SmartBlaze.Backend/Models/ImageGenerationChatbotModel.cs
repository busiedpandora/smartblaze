namespace SmartBlaze.Backend.Models;

public class ImageGenerationChatbotModel : ChatbotModel
{
    private readonly bool _acceptImageSize;
    private readonly string[] _imageSizeSupport;
    private readonly bool _acceptMultipleImagesGenerationAtOnce;
    private readonly int _maxNumberOfGeneratedImagesAtOnce;


    public ImageGenerationChatbotModel(string name, bool acceptSystemInstruction, 
        bool acceptTemperature, float minTemperature, float maxTemperature, 
        bool acceptBase64ImageInput, bool acceptUrlImageInput, 
        bool acceptImageSize, string[] imageSizeSupport, 
        bool acceptMultipleImagesGenerationAtOnce, int maxNumberOfGeneratedImagesAtOnce) 
        : base(name, acceptSystemInstruction, acceptTemperature, minTemperature, maxTemperature, acceptBase64ImageInput, acceptUrlImageInput)
    {
        _acceptImageSize = acceptImageSize;
        _imageSizeSupport = imageSizeSupport;
        _acceptMultipleImagesGenerationAtOnce = acceptMultipleImagesGenerationAtOnce;
        _maxNumberOfGeneratedImagesAtOnce = maxNumberOfGeneratedImagesAtOnce;
    }

    public bool AcceptImageSize => _acceptImageSize;

    public string[] ImageSizeSupport => _imageSizeSupport;

    public bool AcceptMultipleImagesGenerationAtOnce => _acceptMultipleImagesGenerationAtOnce;

    public int MaxNumberOfGeneratedImagesAtOnce => _maxNumberOfGeneratedImagesAtOnce;
}