using JetBrains.Annotations;
using Moq;
using SmartBlaze.Backend.Dtos;
using SmartBlaze.Backend.Models;
using SmartBlaze.Backend.Repositories;
using SmartBlaze.Backend.Services;

namespace SmartBlaze.Backend.Tests.Services;

[TestSubject(typeof(MessageService))]
public class MessageServiceTest
{
    [Fact]
    public void CreateNewUserMessage()
    {
        var messageRepositoryMock = new Mock<IMessageRepository>();
        var messageService = new MessageService(messageRepositoryMock.Object);
        
        MessageDto message = messageService.CreateNewUserMessage("hello", new());
        Assert.Equal("hello", message.Text);
        Assert.Equal(Role.User, message.Role);
    }
    
    [Fact]
    public void CreateNewAssistantTextMessage()
    {
        var messageRepositoryMock = new Mock<IMessageRepository>();
        var messageService = new MessageService(messageRepositoryMock.Object);
        
        MessageDto message = messageService.CreateNewAssistantTextMessage("hello", "ChatGPT", "gpt-4o", "ok");
        Assert.Equal("hello", message.Text);
        Assert.Equal(Role.Assistant, message.Role);
        Assert.Equal("ChatGPT", message.ChatbotName);
        Assert.Equal("gpt-4o", message.ChatbotModel);
        Assert.Equal("ok", message.Status);
    }

    [Fact]
    public void CreateNewAssistantImageMessage()
    {
        var messageRepositoryMock = new Mock<IMessageRepository>();
        var messageService = new MessageService(messageRepositoryMock.Object);
        
        MessageDto message = messageService.CreateNewAssistantImageMessage("text", null, "ChatGPT",
            "dall-e-3", "ok");
        
        Assert.Equal("text", message.Text);
        Assert.Equal(Role.Assistant, message.Role);
        Assert.Equal("ChatGPT", message.ChatbotName);
        Assert.Equal("dall-e-3", message.ChatbotModel);
        Assert.Equal("ok", message.Status);
        Assert.Null(message.MediaDtos);
    }

    [Fact]
    public async void AddNewMessageToChatSession()
    {
        var messageRepositoryMock = new Mock<IMessageRepository>();
        var messageService = new MessageService(messageRepositoryMock.Object);
        var message = new MessageDto();
        var chatSession = new ChatSessionDto();
        chatSession.Id = "id";

        messageRepositoryMock.Setup(repo => repo.SaveMessage(message, chatSession.Id));

        await messageService.AddNewMessageToChatSession(message, chatSession);
        
        messageRepositoryMock.Verify(repo => repo.SaveMessage(message, chatSession.Id), Times.Once);
    }

    [Fact]
    public async void GetMessagesFromChatSession()
    {
        var messageRepositoryMock = new Mock<IMessageRepository>();
        var messageService = new MessageService(messageRepositoryMock.Object);
        var messages = new List<MessageDto>();
        var chatSession = new ChatSessionDto();
        chatSession.Id = "id";
        
        messageRepositoryMock.Setup(repo => repo.GetMessagesFromChatSession(chatSession.Id))
            .ReturnsAsync(messages);

        var result = await messageService.GetMessagesFromChatSession(chatSession);
        
        messageRepositoryMock.Verify(repo => repo.GetMessagesFromChatSession(chatSession.Id), Times.Once);
        
        Assert.Equal(messages, result);
    }
}