using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SmartBlaze.Backend.Controllers;
using SmartBlaze.Backend.Dtos;
using SmartBlaze.Backend.Models;
using SmartBlaze.Backend.Services;

namespace SmartBlaze.Backend.Tests.Controllers;

[TestSubject(typeof(MessageController))]
public class MessageControllerTest
{

    [Fact]
    public async void GetMessagesFromChatSession()
    {
        var chatSessionServiceMock = new Mock<IChatSessionService>();
        var messageServiceMock = new Mock<IMessageService>();
        var chatbotServiceMock = new Mock<IChatbotService>();
        var messageController = new MessageController(chatSessionServiceMock.Object, messageServiceMock.Object, 
            chatbotServiceMock.Object);
        
        var chatSession = new ChatSessionDto
        {
            Title = "chat"
        };
        
        var message = new MessageDto()
        {
            Role = "user",
            Text = "hello"
        };

        var messages = new List<MessageDto>() { message };

        chatSessionServiceMock.Setup(service => service.GetChatSessionById("id"))
            .ReturnsAsync(chatSession);
        messageServiceMock.Setup(service => service.GetMessagesFromChatSession(chatSession))
            .ReturnsAsync(messages);

        var result = await messageController.GetMessagesFromChatSession("id");
        
        chatSessionServiceMock.Verify(service => service.GetChatSessionById("id"), Times.Once);
        messageServiceMock.Verify(service => service.GetMessagesFromChatSession(chatSession), Times.Once);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<List<MessageDto>>(okResult.Value);
        Assert.Single(returnValue);
        Assert.Equal(message, returnValue[0]);
    }
    
    [Fact]
    public async void AddNewUserMessageToChatSession()
    {
        var chatSessionServiceMock = new Mock<IChatSessionService>();
        var messageServiceMock = new Mock<IMessageService>();
        var chatbotServiceMock = new Mock<IChatbotService>();
        var messageController = new MessageController(chatSessionServiceMock.Object, messageServiceMock.Object, 
            chatbotServiceMock.Object);
        
        var chatSession = new ChatSessionDto
        {
            Title = "chat"
        };
        
        var message = new MessageDto()
        {
            Role = "user",
            Text = "hello"
        };
        
        chatSessionServiceMock.Setup(service => service.GetChatSessionById("id"))
            .ReturnsAsync(chatSession);
        messageServiceMock.Setup(service => service.CreateNewUserMessage(message.Text, message.MediaDtos))
            .Returns(message);

        var result = await messageController.AddNewUserMessageToChatSession("id", message);
        
        chatSessionServiceMock.Verify(service => service.GetChatSessionById("id"), Times.Once);
        messageServiceMock.Verify(service => service.CreateNewUserMessage(message.Text, message.MediaDtos), Times.Once);
        messageServiceMock.Verify(service => service.AddNewMessageToChatSession(message, chatSession), Times.Once);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<MessageDto>(okResult.Value);
        Assert.Equal(message, returnValue);
    }
    
    [Fact]
    public async void GenerateNewAssistantTextMessageInChatSession()
    {
        var chatSessionServiceMock = new Mock<IChatSessionService>();
        var messageServiceMock = new Mock<IMessageService>();
        var chatbotServiceMock = new Mock<IChatbotService>();
        var messageController = new MessageController(chatSessionServiceMock.Object, messageServiceMock.Object, 
            chatbotServiceMock.Object);
        
        var chatSession = new ChatSessionDto
        {
            Title = "chat"
        };
        
        var userMessage = new MessageDto()
        {
            Role = "user",
            Text = "hello"
        };

        var assistantMessageInfo = new AssistantMessageInfoDto()
        {
            Status = "ok",
            Text = "hi"
        };
        
        var assistantMessage = new MessageDto()
        {
            Role = "assistant",
            Text = "hi"
        };

        var chatSessionInfo = new ChatSessionInfoDto()
        {
            ChatbotName = "ChatGPT",
            ChatbotModel = "gpt-4o",
            ApiHost = "https://api.openai.com",
            ApiKey = "apikey",
            Messages = new List<MessageDto>() { userMessage }
        };
        
        var chatbotModel = new TextGenerationChatbotModel("gpt-4o", true, true, true,
            true, true, 0.0f, 2.0f,
            true, 100, true);
        
        var chatbot = new ChatGpt("ChatGPT", [chatbotModel], [], true);
        
        chatSessionServiceMock.Setup(service => service.GetChatSessionById("id"))
            .ReturnsAsync(chatSession);
        chatbotServiceMock.Setup(service => service.GetChatbotByName(chatSessionInfo.ChatbotName))
            .Returns(chatbot);
        chatbotServiceMock.Setup(service => service.GenerateTextInChatSession(chatbot, It.IsAny<TextGenerationRequestData>()))
            .ReturnsAsync(assistantMessageInfo);
        messageServiceMock.Setup(service => service.CreateNewAssistantTextMessage(assistantMessageInfo.Text,
                chatSessionInfo.ChatbotName, chatSessionInfo.ChatbotModel, assistantMessageInfo.Status))
            .Returns(assistantMessage);
        
        var result = await messageController.GenerateNewAssistantTextMessageInChatSession("id", chatSessionInfo);
        
        chatSessionServiceMock.Verify(service => service.GetChatSessionById("id"), Times.Once);
        chatbotServiceMock.Verify(service => service.GetChatbotByName(chatSessionInfo.ChatbotName), Times.Once);
        chatbotServiceMock.Verify(service => service.GenerateTextInChatSession(chatbot, 
            It.IsAny<TextGenerationRequestData>()), Times.Once);
        messageServiceMock.Verify(service => service.CreateNewAssistantTextMessage(assistantMessageInfo.Text,
            chatSessionInfo.ChatbotName, chatSessionInfo.ChatbotModel, assistantMessageInfo.Status), Times.Once);
        messageServiceMock.Verify(service => service.AddNewMessageToChatSession(assistantMessage, chatSession),
            Times.Once);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<MessageDto>(okResult.Value);
        Assert.Equal(assistantMessage, returnValue);
    }

    [Fact]
    public void GetNewAssistantMessageWithEmptyContent()
    {
        var chatSessionServiceMock = new Mock<IChatSessionService>();
        var messageServiceMock = new Mock<IMessageService>();
        var chatbotServiceMock = new Mock<IChatbotService>();
        var messageController = new MessageController(chatSessionServiceMock.Object, messageServiceMock.Object, 
            chatbotServiceMock.Object);
        
        var chatSessionInfo = new ChatSessionInfoDto()
        {
            ChatbotName = "ChatGPT",
            ChatbotModel = "gpt-4o",
            ApiHost = "https://api.openai.com",
            ApiKey = "apikey",
        };
        
        var assistantMessage = new MessageDto()
        {
            Role = "assistant",
            Text = "hi"
        };

        messageServiceMock.Setup(service => service.CreateNewAssistantTextMessage("", chatSessionInfo.ChatbotName,
                chatSessionInfo.ChatbotModel, "ok"))
            .Returns(assistantMessage);
        
        var result = messageController.GetNewAssistantMessageWithEmptyContent("id", chatSessionInfo);
        
        messageServiceMock.Verify(service => service.CreateNewAssistantTextMessage("", chatSessionInfo.ChatbotName,
            chatSessionInfo.ChatbotModel, "ok"), Times.Once);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<MessageDto>(okResult.Value);
        Assert.Equal(assistantMessage, returnValue);
    }
    
    [Fact]
    public async void GenerateNewAssistantTextMessageInChatSessionStreamEnabled()
    {
        var chatSessionServiceMock = new Mock<IChatSessionService>();
        var messageServiceMock = new Mock<IMessageService>();
        var chatbotServiceMock = new Mock<IChatbotService>();
        var messageController = new MessageController(chatSessionServiceMock.Object, messageServiceMock.Object, 
            chatbotServiceMock.Object);
        
        var chatSession = new ChatSessionDto
        {
            Title = "chat"
        };
        
        var userMessage = new MessageDto()
        {
            Role = "user",
            Text = "hello"
        };

        var assistantMessageInfo = new AssistantMessageInfoDto()
        {
            Status = "ok",
            Text = "hi"
        };
        
        var assistantMessage = new MessageDto()
        {
            Role = "assistant",
            Text = "hi"
        };

        var chatSessionInfo = new ChatSessionInfoDto()
        {
            ChatbotName = "ChatGPT",
            ChatbotModel = "gpt-4o",
            ApiHost = "https://api.openai.com",
            ApiKey = "apikey",
            Messages = new List<MessageDto>() { userMessage }
        };
        
        var chatbotModel = new TextGenerationChatbotModel("gpt-4o", true, true, true,
            true, true, 0.0f, 2.0f,
            true, 100, true);
        
        var chatbot = new ChatGpt("ChatGPT", [chatbotModel], [], true);
        
        chatSessionServiceMock.Setup(service => service.GetChatSessionById("id"))
            .ReturnsAsync(chatSession);
        chatbotServiceMock.Setup(service => service.GetChatbotByName(chatSessionInfo.ChatbotName))
            .Returns(chatbot);
        chatbotServiceMock.Setup(service => service.GenerateTextStreamInChatSession(chatbot, It.IsAny<TextGenerationRequestData>()))
            .Returns(GetTextStream());
        messageServiceMock.Setup(service => service.CreateNewAssistantTextMessage("FirstSecond",
                chatSessionInfo.ChatbotName, chatSessionInfo.ChatbotModel, assistantMessageInfo.Status))
            .Returns(assistantMessage);

        var index = 0;
        var streamOutput = new string?[2];
        
        await foreach (var s in messageController.GenerateNewAssistantTextMessageInChatSessionStreamEnabled("id",
                           chatSessionInfo))
        {
            if (index < streamOutput.Length)
            {
                streamOutput[index++] = s;
            }
        }
        
        chatSessionServiceMock.Verify(service => service.GetChatSessionById("id"), Times.Once);
        chatbotServiceMock.Verify(service => service.GetChatbotByName(chatSessionInfo.ChatbotName), Times.Once);
        chatbotServiceMock.Verify(service => service.GenerateTextStreamInChatSession(chatbot, 
            It.IsAny<TextGenerationRequestData>()), Times.Once);
        messageServiceMock.Verify(service => service.CreateNewAssistantTextMessage("FirstSecond",
            chatSessionInfo.ChatbotName, chatSessionInfo.ChatbotModel, assistantMessageInfo.Status), Times.Once);
        messageServiceMock.Verify(service => service.AddNewMessageToChatSession(assistantMessage, chatSession),
            Times.Once);
        Assert.Equal("First", streamOutput[0]);
        Assert.Equal("Second", streamOutput[1]);
    }
    
    [Fact]
    public async void GenerateNewAssistantImageMessageFromChatSession()
    {
        var chatSessionServiceMock = new Mock<IChatSessionService>();
        var messageServiceMock = new Mock<IMessageService>();
        var chatbotServiceMock = new Mock<IChatbotService>();
        var messageController = new MessageController(chatSessionServiceMock.Object, messageServiceMock.Object, 
            chatbotServiceMock.Object);
        
        var chatSession = new ChatSessionDto
        {
            Title = "chat"
        };
        
        var userMessage = new MessageDto()
        {
            Role = "user",
            Text = "generate a cat"
        };

        var assistantMessageInfo = new AssistantMessageInfoDto()
        {
            Status = "ok",
            Text = "hi",
            MediaDtos = new List<MediaDto>()
        };
        
        var assistantMessage = new MessageDto()
        {
            Role = "assistant",
            Text = "hi"
        };

        var chatSessionInfo = new ChatSessionInfoDto()
        {
            ChatbotName = "ChatGPT",
            ChatbotModel = "dall-e-3",
            ApiHost = "https://api.openai.com",
            ApiKey = "apikey",
            LastUserMessage = userMessage
        };

        var chatbotModel = new ImageGenerationChatbotModel("dall-e-3", false,
            false, false, true, ["1024x1024", "1024x1792", "1792x1024"],
            false, 1);
        
        var chatbot = new ChatGpt("ChatGPT", [], [chatbotModel], true);
        
        chatSessionServiceMock.Setup(service => service.GetChatSessionById("id"))
            .ReturnsAsync(chatSession);
        chatbotServiceMock.Setup(service => service.GetChatbotByName(chatSessionInfo.ChatbotName))
            .Returns(chatbot);
        chatbotServiceMock.Setup(service => service.GenerateImageInChatSession(chatbot, It.IsAny<ImageGenerationRequestData>()))
            .ReturnsAsync(assistantMessageInfo);
        messageServiceMock.Setup(service => service.CreateNewAssistantImageMessage(assistantMessageInfo.Text, assistantMessageInfo.MediaDtos,
                chatSessionInfo.ChatbotName, chatSessionInfo.ChatbotModel, assistantMessageInfo.Status))
            .Returns(assistantMessage);
        
        var result = await messageController.GenerateNewAssistantImageMessageFromChatSession("id", chatSessionInfo);
        
        chatSessionServiceMock.Verify(service => service.GetChatSessionById("id"), Times.Once);
        chatbotServiceMock.Verify(service => service.GetChatbotByName(chatSessionInfo.ChatbotName), Times.Once);
        chatbotServiceMock.Verify(service => service.GenerateImageInChatSession(chatbot, 
            It.IsAny<ImageGenerationRequestData>()), Times.Once);
        messageServiceMock.Verify(service => service.CreateNewAssistantImageMessage(assistantMessageInfo.Text, assistantMessageInfo.MediaDtos,
            chatSessionInfo.ChatbotName, chatSessionInfo.ChatbotModel, assistantMessageInfo.Status), Times.Once);
        messageServiceMock.Verify(service => service.AddNewMessageToChatSession(assistantMessage, chatSession),
            Times.Once);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<MessageDto>(okResult.Value);
        Assert.Equal(assistantMessage, returnValue);
    }
    
    private static async IAsyncEnumerable<string> GetTextStream()
    {
        yield return "First";
        await Task.Delay(10);
        yield return "Second";
    }
}