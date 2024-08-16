namespace SmartBlaze.Backend.Models;

public class ImageGenerationChatbotModel : ChatbotModel
{
    private readonly bool _acceptImageSize;
    private readonly string[] _imageSizeSupport;
    private readonly bool _acceptMultipleImagesGeneration;
    private readonly int _maxImagesGenerated;


    public ImageGenerationChatbotModel(string name, 
        bool acceptBase64ImageInput, bool acceptUrlImageInput, bool acceptTextFileInput,
        bool acceptImageSize, string[] imageSizeSupport, 
        bool acceptMultipleImagesGeneration, int maxImagesGenerated) 
        : base(name, acceptBase64ImageInput, acceptUrlImageInput, acceptTextFileInput)
    {
        _acceptImageSize = acceptImageSize;
        _imageSizeSupport = imageSizeSupport;
        _acceptMultipleImagesGeneration = acceptMultipleImagesGeneration;
        _maxImagesGenerated = maxImagesGenerated;
    }

    public bool AcceptImageSize => _acceptImageSize;

    public string[] ImageSizeSupport => _imageSizeSupport;

    public bool AcceptMultipleImagesGeneration => _acceptMultipleImagesGeneration;

    public int MaxImagesGenerated => _maxImagesGenerated;
}