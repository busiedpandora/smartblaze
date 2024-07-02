using SmartBlaze.Backend.Dtos;
using SmartBlaze.Backend.Models;

namespace SmartBlaze.Backend.Services;

public class MessageService
{
    public MessageDto CreateNewUserMessage(string content)
    {
        return new MessageDto()
        {
            Content = content,
            Role = Role.User,
            CreationDate = DateTime.Now
        };
    }
    
    public MessageDto CreateNewAssistantMessage(string content)
    {
        return new MessageDto()
        {
            Content = content,
            Role = Role.Assistant,
            CreationDate = DateTime.Now
        };
    }
    
    public MessageDto CreateNewSystemMessage(string content)
    {
        return new MessageDto()
        {
            Content = content,
            Role = Role.System,
            CreationDate = DateTime.Now
        };
    }
}