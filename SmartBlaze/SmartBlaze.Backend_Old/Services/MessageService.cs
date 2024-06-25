using SmartBlaze.Backend.Models;

namespace SmartBlaze.Backend.Services;

public class MessageService
{
    public Message CreateNewUserMessage(string content)
    {
        return new Message(content, Role.User, DateTime.Now);
    }
    
    public Message CreateNewAssistantMessage(string content)
    {
        return new Message(content, Role.Assistant, DateTime.Now);
    }
    
    public Message CreateNewSystemMessage(string content)
    {
        return new Message(content, Role.System, DateTime.Now);
    }
}