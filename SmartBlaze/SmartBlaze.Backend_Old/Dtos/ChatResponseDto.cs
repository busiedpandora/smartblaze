using System.Text.Json.Serialization;

namespace SmartBlaze.Backend.Dtos;

public class ChatResponseDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("choices")]
    public List<ChoiceDto>? Choices { get; set; }
    
    [JsonPropertyName("created")]
    public long? Created { get; set; }
    
    [JsonPropertyName("model")]
    public string? Model { get; set; }
    
    [JsonPropertyName("object")]
    public string? Object { get; set; }
}