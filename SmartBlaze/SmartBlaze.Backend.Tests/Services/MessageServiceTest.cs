using JetBrains.Annotations;
using SmartBlaze.Backend.Models;
using SmartBlaze.Backend.Services;

namespace SmartBlaze.Backend.Tests.Services;

[TestSubject(typeof(MessageService))]
public class MessageServiceTest
{
    private MessageService _messageService;

    
    public MessageServiceTest()
    {
        _messageService = new MessageService();
    }

    [Fact]
    public void CreateNewUserMessage()
    {
        Message message = _messageService.CreateNewUserMessage("hello");
        Assert.Equal("hello", message.Content);
        Assert.Equal(Role.User, message.Role);
    }
    
    [Fact]
    public void CreateNewAssistantMessage()
    {
        Message message = _messageService.CreateNewAssistantMessage("hello");
        Assert.Equal("hello", message.Content);
        Assert.Equal(Role.Assistant, message.Role);
    }
    
    [Fact]
    public void CreateNewSystemMessage()
    {
        Message message = _messageService.CreateNewSystemMessage("hello");
        Assert.Equal("hello", message.Content);
        Assert.Equal(Role.System, message.Role);
    }
}