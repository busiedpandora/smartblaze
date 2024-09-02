using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SmartBlaze.Backend.Controllers;
using SmartBlaze.Backend.Dtos;
using SmartBlaze.Backend.Models;
using SmartBlaze.Backend.Services;

namespace SmartBlaze.Backend.Tests.Controllers;

[TestSubject(typeof(ChatbotController))]
public class ChatbotControllerTest
{

    [Fact]
    public void GetAllChatbotsAndModels()
    {
        var chatbotServiceMock = new Mock<IChatbotService>();

        var chatbotController = new ChatbotController(chatbotServiceMock.Object);

        var chatbots = new List<Chatbot>()
        {
            new ChatGpt("ChatGPT", [], [], true),
            new ChatGpt("Google Gemini", [], [], false),
        };

        chatbotServiceMock.Setup(service => service.GetAllChatbots())
            .Returns(chatbots);

        var result = chatbotController.GetAllChatbotsAndModels();
        
        chatbotServiceMock.Verify(service => service.GetAllChatbots(), Times.Once);
        
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<List<ChatbotDto>>(okResult.Value);
        Assert.Equal(chatbots.Count, returnValue.Count);
    }
}