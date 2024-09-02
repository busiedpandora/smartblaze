using JetBrains.Annotations;
using Moq;
using SmartBlaze.Backend.Dtos;
using SmartBlaze.Backend.Repositories;
using SmartBlaze.Backend.Services;

namespace SmartBlaze.Backend.Tests.Services;

[TestSubject(typeof(ChatSessionService))]
public class ChatSessionServiceTest
{
    [Fact]
    public async void GetAllChatSessions()
    {
        var chatSessionRepositoryMock = new Mock<IChatSessionRepository>();
        var chatSessionService = new ChatSessionService(chatSessionRepositoryMock.Object);
        var chatSessions = new List<ChatSessionDto>();
        
        chatSessionRepositoryMock.Setup(repo => repo.GetAllChatSessions("id"))
            .ReturnsAsync(chatSessions);
        
        var result = await chatSessionService.GetAllChatSessions("id");
        
        chatSessionRepositoryMock.Verify(repo => repo.GetAllChatSessions("id"), Times.Once);
        Assert.Equal(chatSessions, result);
    }
    
    [Fact]
    public async void GetChatSessionById()
    {
        var chatSessionRepositoryMock = new Mock<IChatSessionRepository>();
        var chatSessionService = new ChatSessionService(chatSessionRepositoryMock.Object);
        var chatSession = new ChatSessionDto();
        
        chatSessionRepositoryMock.Setup(repo => repo.GetChatSessionById("id"))
            .ReturnsAsync(chatSession);

        var result = await chatSessionService.GetChatSessionById("id");
        
        chatSessionRepositoryMock.Verify(repo => repo.GetChatSessionById("id"), Times.Once);
        Assert.Equal(chatSession, result);
    }

    [Fact]
    public void CreateNewChatSession()
    {
        var chatSessionRepositoryMock = new Mock<IChatSessionRepository>();
        var chatSessionService = new ChatSessionService(chatSessionRepositoryMock.Object);

        var result = chatSessionService.CreateNewChatSession("title");
        
        Assert.Equal("title", result.Title);
    }
    
    [Fact]
    public async void AddChatSession()
    {
        var chatSessionRepositoryMock = new Mock<IChatSessionRepository>();
        var chatSessionService = new ChatSessionService(chatSessionRepositoryMock.Object);
        var chatSession = new ChatSessionDto();
        
        chatSessionRepositoryMock.Setup(repo => repo.SaveChatSession(chatSession, "id"))
            .ReturnsAsync(chatSession);

        var result = await chatSessionService.AddChatSession(chatSession, "id");
        
        chatSessionRepositoryMock.Verify(repo => repo.SaveChatSession(chatSession, "id"), Times.Once);
        Assert.Equal(chatSession, result);
    }
    
    [Fact]
    public async void EditChatSession()
    {
        var chatSessionRepositoryMock = new Mock<IChatSessionRepository>();
        var chatSessionService = new ChatSessionService(chatSessionRepositoryMock.Object);
        var chatSession = new ChatSessionDto();

        chatSessionRepositoryMock.Setup(repo => repo.EditChatSession(chatSession));

        await chatSessionService.EditChatSession(chatSession);
        
        chatSessionRepositoryMock.Verify(repo => repo.EditChatSession(chatSession), Times.Once);
    }
}