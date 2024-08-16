using System.Text.Json.Serialization;

namespace SmartBlaze.Backend.Dtos;

public class ChatbotModelDto
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }
    [JsonPropertyName("type")]
    public required string Type { get; set; }
    [JsonPropertyName("acceptBase64ImageInput")]
    public bool AcceptBase64ImageInput { get; set; }
    [JsonPropertyName("acceptUrlImageInput")]
    public bool AcceptUrlImageInput { get; set; }
    
    [JsonPropertyName("acceptTextFileInput")]
    public bool AcceptTextFileInput { get; set; }
    
    //text generation
    [JsonPropertyName("acceptSystemInstruction")]
    public bool AcceptSystemInstruction { get; set; }
    [JsonPropertyName("acceptTemperature")]
    public bool AcceptTemperature { get; set; }
    [JsonPropertyName("minTemperature")]
    public float MinTemperature { get; set; }
    [JsonPropertyName("maxTemperature")]
    public float MaxTemperature { get; set; }
    [JsonPropertyName("acceptTextStream")]
    public bool AcceptTextStream { get; set; }
    [JsonPropertyName("textStreamDelay")]
    public int TextStreamDelay { get; set; }
    [JsonPropertyName("acceptImageVision")]
    public bool AcceptImageVision { get; set; }
    
    //image generation
    [JsonPropertyName("acceptImageSize")]
    public bool AcceptImageSize { get; set; }
    [JsonPropertyName("imageSizeSupport")]
    public string[] ImageSizeSupport { get; set; } = [];
    [JsonPropertyName("acceptMultipleImagesGeneration")]
    public bool AcceptMultipleImagesGeneration { get; set; }
    [JsonPropertyName("maxImagesGenerated")]
    public int MaxImagesGenerated { get; set; }
}