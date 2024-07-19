using System.Text.Json.Serialization;

namespace SmartBlaze.Frontend.Dtos;

public class MessageDto
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }
    
    [JsonPropertyName("role")]
    public string? Role { get; set; }
    
    [JsonPropertyName("creationDate")]
    public DateTime? CreationDate { get; set; }

    [JsonPropertyName("medias")]
    public List<MediaDto>? MediaDtos { get; set; }

    public static MessageDto ToMessageDto(string? content, string? role, DateTime? creationDate)
    {
        MessageDto messageDto = new MessageDto()
        {
            Text = content,
            Role = role,
            CreationDate = creationDate
        };

        return messageDto;
    }
}