using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SmartBlaze.Backend.Controllers;
using SmartBlaze.Backend.Dtos;
using SmartBlaze.Backend.Services;

namespace SmartBlaze.Backend.Tests.Controllers;

[TestSubject(typeof(ChatSessionController))]
public class ChatSessionControllerTest
{

    [Fact]
    public async void GetAllChatSessions()
    {
        var chatSessionServiceMock = new Mock<IChatSessionService>();
        var configurationServiceMock = new Mock<IConfigurationService>();
        var chatbotServiceMock = new Mock<IChatbotService>();
        var chatSessionController =
            new ChatSessionController(chatSessionServiceMock.Object, configurationServiceMock.Object, chatbotServiceMock.Object);

        var chatSession = new ChatSessionDto
        {
            Title = "chat"
        };

        chatSessionServiceMock.Setup(service => service.GetAllChatSessions("id"))
            .ReturnsAsync(new List<ChatSessionDto>() { chatSession });
        
        var result = await chatSessionController.GetAllChatSessions("id");
        
        chatSessionServiceMock.Verify(service => service.GetAllChatSessions("id"), Times.Once);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<List<ChatSessionDto>>(okResult.Value);
        Assert.Single(returnValue);
        Assert.Equal(chatSession, returnValue[0]);
    }
    
    [Fact]
    public async void AddNewChatSession()
    {
        var chatSessionServiceMock = new Mock<IChatSessionService>();
        var configurationServiceMock = new Mock<IConfigurationService>();
        var chatbotServiceMock = new Mock<IChatbotService>();
        var chatSessionController =
            new ChatSessionController(chatSessionServiceMock.Object, configurationServiceMock.Object, chatbotServiceMock.Object);

        var chatSession = new ChatSessionDto
        {
            Title = "chat"
        };

        chatSessionServiceMock.Setup(service => service.CreateNewChatSession(chatSession.Title))
            .Returns(chatSession);
        chatSessionServiceMock.Setup(service => service.AddChatSession(chatSession, "id"))
            .ReturnsAsync(chatSession);
        
        var result = await chatSessionController.AddNewChatSession("id", chatSession);
        
        chatSessionServiceMock.Verify(service => service.CreateNewChatSession(chatSession.Title), Times.Once);
        chatSessionServiceMock.Verify(service => service.AddChatSession(chatSession, "id"), Times.Once);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<ChatSessionDto>(okResult.Value);
        Assert.Equal("chat", returnValue.Title);
    }
    
    [Fact]
    public async void DeleteChatSession()
    {
        var chatSessionServiceMock = new Mock<IChatSessionService>();
        var configurationServiceMock = new Mock<IConfigurationService>();
        var chatbotServiceMock = new Mock<IChatbotService>();
        var chatSessionController =
            new ChatSessionController(chatSessionServiceMock.Object, configurationServiceMock.Object, chatbotServiceMock.Object);

        var chatSession = new ChatSessionDto
        {
            Title = "chat"
        };

        chatSessionServiceMock.Setup(service => service.GetChatSessionById("id"))
            .ReturnsAsync(chatSession);
        
        
        var result = await chatSessionController.DeleteChatSession("id");
        
        chatSessionServiceMock.Verify(service => service.GetChatSessionById("id"), Times.Once);
        configurationServiceMock.Verify(service => service.DeleteChatSessionAndItsConfiguration("id"), Times.Once);
        Assert.IsType<OkResult>(result);
    }
    
    [Fact]
    public async void EditChatSession()
    {
        var chatSessionServiceMock = new Mock<IChatSessionService>();
        var configurationServiceMock = new Mock<IConfigurationService>();
        var chatbotServiceMock = new Mock<IChatbotService>();
        var chatSessionController =
            new ChatSessionController(chatSessionServiceMock.Object, configurationServiceMock.Object, chatbotServiceMock.Object);

        var chatSession = new ChatSessionDto
        {
            Title = "chat"
        };

        var chatSessionEdit = new ChatSessionEditDto
        {
            Title = "chat2"
        };
        
        chatSessionServiceMock.Setup(service => service.GetChatSessionById("id"))
            .ReturnsAsync(chatSession);
        
        
        var result = await chatSessionController.EditChatSession("id", chatSessionEdit);
        
        chatSessionServiceMock.Verify(service => service.GetChatSessionById("id"), Times.Once);
        chatSessionServiceMock.Verify(service => service.EditChatSession(chatSession), Times.Once);
        Assert.IsType<OkResult>(result);
        Assert.Equal("chat2", chatSession.Title);
    }
}