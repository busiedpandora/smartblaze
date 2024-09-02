using JetBrains.Annotations;
using Moq;
using SmartBlaze.Backend.Dtos;
using SmartBlaze.Backend.Models;
using SmartBlaze.Backend.Services;

namespace SmartBlaze.Backend.Tests.Services;

[TestSubject(typeof(ChatbotService))]
public class ChatbotServiceTest
{

    [Fact]
    public void GetAllChatbots()
    {
        var httpClientMock = new Mock<HttpClient>();
        var chatbotService = new ChatbotService(httpClientMock.Object);

        var result = chatbotService.GetAllChatbots();
        
        Assert.Equal(2, result.Count);

        var chatGpt = result[0];
        Assert.Equal("ChatGPT", chatGpt.Name);

        var chatGptTextGenerationModels = chatGpt.TextGenerationChatbotModels;
        Assert.Equal(11, chatGptTextGenerationModels.Count);
        
        var chatGptImageGenerationModels = chatGpt.ImageGenerationChatbotModels;
        Assert.Equal(2, chatGptImageGenerationModels.Count);
        
        var gemini = result[1];
        Assert.Equal("Google Gemini", gemini.Name);

        var geminiTextGenerationModels = gemini.TextGenerationChatbotModels;
        Assert.Equal(10, geminiTextGenerationModels.Count);
        
        var geminiImageGenerationModels = gemini.ImageGenerationChatbotModels;
        Assert.Empty(geminiImageGenerationModels);
    }

    [Fact]
    public void GetChatbotByName()
    {
        var httpClientMock = new Mock<HttpClient>();
        var chatbotService = new ChatbotService(httpClientMock.Object);

        var result1 = chatbotService.GetChatbotByName("ChatGPT");
        Assert.NotNull(result1);
        Assert.Equal("ChatGPT", result1.Name);
        
        var result2 = chatbotService.GetChatbotByName("Google Gemini");
        Assert.NotNull(result2);
        Assert.Equal("Google Gemini", result2.Name);
    }

    [Fact]
    public async void GenerateTextInChatSession()
    {
        var textGenerationModels = new List<TextGenerationChatbotModel>();
        var imageGenerationModels = new List<ImageGenerationChatbotModel>();
        
        var httpClientMock = new Mock<HttpClient>();
        var chatbotMock = new Mock<Chatbot>("ChatGPT", textGenerationModels, imageGenerationModels, true);
        
        var chatbotModel = new TextGenerationChatbotModel("gpt-4o", true, true, true, 
            true, true, 0.0f, 2.0f, 
            true, 100, true);
        
        var chatbotService = new ChatbotService(httpClientMock.Object);
        
        var textGenerationRequestData = new TextGenerationRequestData
        {
            Messages = new List<MessageDto>(),
            ChatbotModel = chatbotModel,
            ApiHost = "",
            ApiKey = ""
        };

        var assistantMessageInfo = new AssistantMessageInfoDto()
        {
            Status = "ok",
            Text = "hello"
        };

        chatbotMock.Setup(chatbot => chatbot.GenerateText(textGenerationRequestData, httpClientMock.Object))
            .ReturnsAsync(assistantMessageInfo);

        var result = await chatbotService.GenerateTextInChatSession(chatbotMock.Object, textGenerationRequestData);
        
        chatbotMock.Verify(chatbot => chatbot.GenerateText(textGenerationRequestData, httpClientMock.Object), Times.Once);
        
        Assert.Equal("ok", result.Status);
        Assert.Equal("hello", result.Text);
    }
    
    [Fact]
    public async void GenerateTextStreamInChatSession()
    {
        var textGenerationModels = new List<TextGenerationChatbotModel>();
        var imageGenerationModels = new List<ImageGenerationChatbotModel>();
        
        var httpClientMock = new Mock<HttpClient>();
        var chatbotMock = new Mock<Chatbot>("ChatGPT", textGenerationModels, imageGenerationModels, true);
        
        var chatbotModel = new TextGenerationChatbotModel("gpt-4o", true, true, true, 
            true, true, 0.0f, 2.0f, 
            true, 100, true);
        
        var chatbotService = new ChatbotService(httpClientMock.Object);
        
        var textGenerationRequestData = new TextGenerationRequestData
        {
            Messages = new List<MessageDto>(),
            ChatbotModel = chatbotModel,
            ApiHost = "",
            ApiKey = ""
        };

        chatbotMock.Setup(
                chatbot => chatbot.GenerateTextStreamEnabled(textGenerationRequestData, httpClientMock.Object))
            .Returns(GetTextStream());

        
        var index = 0;
        var streamOutput = new string[2];
        
        await foreach (var s in chatbotService.GenerateTextStreamInChatSession(chatbotMock.Object,
                           textGenerationRequestData))
        {
            if (index < streamOutput.Length)
            {
                streamOutput[index++] = s;
            }
        }

        chatbotMock.Verify(chatbot => chatbot.GenerateTextStreamEnabled(textGenerationRequestData, httpClientMock.Object), Times.Once);
        Assert.Equal("First", streamOutput[0]);
        Assert.Equal("Second", streamOutput[1]);
    }
    
    [Fact]
    public async void GenerateImageInChatSession()
    {
        var textGenerationModels = new List<TextGenerationChatbotModel>();
        var imageGenerationModels = new List<ImageGenerationChatbotModel>();
        
        var httpClientMock = new Mock<HttpClient>();
        var chatbotMock = new Mock<Chatbot>("ChatGPT", textGenerationModels, imageGenerationModels, true);
        
        var chatbotModel = new ImageGenerationChatbotModel("dall-e-3", false, 
            false, false, true, ["1024x1024", "1024x1792", "1792x1024"], 
            false, 1);
        
        var chatbotService = new ChatbotService(httpClientMock.Object);
        
        var imageGenerationRequestData = new ImageGenerationRequestData()
        {
            LastUserMessage = new MessageDto(),
            ChatbotModel = chatbotModel,
            ApiHost = "",
            ApiKey = ""
        };

        var media = new MediaDto()
        {
            Name = "image"
        };

        var assistantMessageInfo = new AssistantMessageInfoDto()
        {
            Status = "ok",
            Text = "hello",
            MediaDtos = new List<MediaDto>() {media}
        };

        chatbotMock.Setup(chatbot => chatbot.GenerateImage(imageGenerationRequestData, httpClientMock.Object))
            .ReturnsAsync(assistantMessageInfo);

        var result = await chatbotService.GenerateImageInChatSession(chatbotMock.Object, imageGenerationRequestData);
        
        chatbotMock.Verify(chatbot => chatbot.GenerateImage(imageGenerationRequestData, httpClientMock.Object), Times.Once);
        
        Assert.Equal("ok", result.Status);
        Assert.Equal("hello", result.Text);
        Assert.NotNull(result.MediaDtos);
        Assert.Single(result.MediaDtos);
        Assert.Equal("image", result.MediaDtos[0].Name);
    }
    
    [Fact]
    public async void EntitleChatSessionFromUserMessage()
    {
        var textGenerationModels = new List<TextGenerationChatbotModel>();
        var imageGenerationModels = new List<ImageGenerationChatbotModel>();
        
        var httpClientMock = new Mock<HttpClient>();
        var chatbotMock = new Mock<Chatbot>("ChatGPT", textGenerationModels, imageGenerationModels, true);
        
        var chatbotModel = new TextGenerationChatbotModel("gpt-4o", true, true, true, 
            true, true, 0.0f, 2.0f, 
            true, 100, true);
        
        var chatbotService = new ChatbotService(httpClientMock.Object);
        
        var textGenerationRequestData = new TextGenerationRequestData
        {
            Messages = new List<MessageDto>(),
            ChatbotModel = chatbotModel,
            ApiHost = "",
            ApiKey = ""
        };

        var assistantMessageInfo = new AssistantMessageInfoDto()
        {
            Status = "ok",
            Text = "hello"
        };

        chatbotMock.Setup(chatbot => chatbot.EntitleChatSession(textGenerationRequestData, httpClientMock.Object))
            .ReturnsAsync(assistantMessageInfo);

        var result = await chatbotService.EntitleChatSessionFromUserMessage(chatbotMock.Object, textGenerationRequestData);
        
        chatbotMock.Verify(chatbot => chatbot.EntitleChatSession(textGenerationRequestData, httpClientMock.Object), Times.Once);
        
        Assert.Equal("ok", result.Status);
        Assert.Equal("hello", result.Text);
    }
    
    private static async IAsyncEnumerable<string> GetTextStream()
    {
        yield return "First";
        await Task.Delay(10);
        yield return "Second";
    }
}