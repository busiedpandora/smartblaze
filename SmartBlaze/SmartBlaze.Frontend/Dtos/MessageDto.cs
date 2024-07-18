using System.Text.Json.Serialization;

namespace SmartBlaze.Frontend.Dtos;

public class MessageDto
{
    [JsonPropertyName("content")]
    public string? Content { get; set; }
    
    [JsonPropertyName("role")]
    public string? Role { get; set; }
    
    [JsonPropertyName("creationDate")]
    public DateTime? CreationDate { get; set; }

    [JsonPropertyName("userImage")]
    public List<UserImageDto>? UserImageDtos { get; set; }

    public static MessageDto ToMessageDto(string? content, string? role, DateTime? creationDate)
    {
        MessageDto messageDto = new MessageDto()
        {
            Content = content,
            Role = role,
            CreationDate = creationDate
        };

        return messageDto;
    }
}