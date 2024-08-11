using JetBrains.Annotations;
using SmartBlaze.Backend.Dtos;
using SmartBlaze.Backend.Models;
using SmartBlaze.Backend.Repositories;
using SmartBlaze.Backend.Services;

namespace SmartBlaze.Backend.Tests.Services;

[TestSubject(typeof(MessageService))]
public class MessageServiceTest
{
    private MessageService _messageService;

    
    public MessageServiceTest()
    {
        _messageService = new MessageService(new MessageRepository());
    }

    [Fact]
    public void CreateNewUserMessage()
    {
        MessageDto message = _messageService.CreateNewUserMessage("hello", new());
        Assert.Equal("hello", message.Text);
        Assert.Equal(Role.User, message.Role);
    }
    
    [Fact]
    public void CreateNewAssistantMessage()
    {
        MessageDto message = _messageService.CreateNewAssistantTextMessage("hello", "ChatGPT", "gpt-4o");
        Assert.Equal("hello", message.Text);
        Assert.Equal(Role.Assistant, message.Role);
        Assert.Equal("ChatGPT", message.ChatbotName);
        Assert.Equal("gpt-4o", message.ChatbotModel);
    }
}