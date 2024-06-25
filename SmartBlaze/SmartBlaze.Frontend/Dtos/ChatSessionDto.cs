using System.Text.Json.Serialization;
using SmartBlaze.Frontend.Models;

namespace SmartBlaze.Frontend.Dtos;

public class ChatSessionDto
{
    [JsonPropertyName("id")]
    public long? Id { get; set; }
    
    [JsonPropertyName("title")]
    public string? Title { get; set; }
    
    [JsonPropertyName("creationDate")]
    public DateTime? CreationDate { get; set; }
    
    public List<MessageDto>? Messages { get; set; }
    
    
    public static ChatSessionDto ToChatSessionDto(long? id, string? title, DateTime? creationDate, List<Message>? messages)
    {
        List<MessageDto>? messagesDto = null;
        
        if (messages is not null)
        { 
            messagesDto = messages
                .Select(m => MessageDto.ToMessageDto(m.Content, m.Role, m.CreationDate))
                .ToList();
        }
        
        ChatSessionDto chatSessionDto = new ChatSessionDto
        {
            Id = id,
            Title = title,
            CreationDate = creationDate,
            Messages = messagesDto
        };
        
        return chatSessionDto;
    }
}