using System.Text.Json.Serialization;

namespace SmartBlaze.Backend.Dtos;

public class ChatRequestDto
{
    public ChatRequestDto(string? model, List<MessageDto>? messages)
    {
        Model = model;
        Messages = messages;
    }

    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [JsonPropertyName("messages")]
    public List<MessageDto>? Messages { get; set; }
}