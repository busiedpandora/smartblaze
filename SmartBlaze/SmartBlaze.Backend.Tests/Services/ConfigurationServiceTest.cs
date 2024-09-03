using JetBrains.Annotations;
using Moq;
using SmartBlaze.Backend.Dtos;
using SmartBlaze.Backend.Repositories;
using SmartBlaze.Backend.Services;

namespace SmartBlaze.Backend.Tests.Services;

[TestSubject(typeof(ConfigurationService))]
public class ConfigurationServiceTest
{

    [Fact]
    public async void GetChatbotDefaultConfiguration()
    {
        var configurationRepositoryMock = new Mock<IConfigurationRepository>();
        var configurationService = new ConfigurationService(configurationRepositoryMock.Object);
        var chatbotDefaultConfiguration = new ChatbotDefaultConfigurationDto();
        chatbotDefaultConfiguration.ChatbotName = "ChatGPT";

        configurationRepositoryMock.Setup(repo => repo.GetChatbotDefaultConfiguration("ChatGPT", "id"))
            .ReturnsAsync(chatbotDefaultConfiguration);

        var result = await configurationService.GetChatbotDefaultConfiguration("ChatGPT", "id");
        
        configurationRepositoryMock.Verify(repo => repo.GetChatbotDefaultConfiguration("ChatGPT", "id"), 
            Times.Once);
        Assert.Equal(chatbotDefaultConfiguration, result);
    }

    [Fact]
    public async void SaveChatbotDefaultConfiguration()
    {
        var configurationRepositoryMock = new Mock<IConfigurationRepository>();
        var configurationService = new ConfigurationService(configurationRepositoryMock.Object);
        var chatbotDefaultConfiguration = new ChatbotDefaultConfigurationDto();
        chatbotDefaultConfiguration.ChatbotName = "ChatGPT";

        await configurationService.SaveChatbotDefaultConfiguration(chatbotDefaultConfiguration, "id");
        
        configurationRepositoryMock.Verify(repo => repo.SaveChatbotDefaultConfiguration(chatbotDefaultConfiguration, 
            "id"),Times.Once);
    }
    
    [Fact]
    public async void EditChatbotDefaultConfiguration()
    {
        var configurationRepositoryMock = new Mock<IConfigurationRepository>();
        var configurationService = new ConfigurationService(configurationRepositoryMock.Object);
        var chatbotDefaultConfiguration = new ChatbotDefaultConfigurationDto();
        chatbotDefaultConfiguration.ChatbotName = "ChatGPT";

        await configurationService.EditChatbotDefaultConfiguration(chatbotDefaultConfiguration);
        
        configurationRepositoryMock.Verify(repo => repo.EditChatbotDefaultConfiguration(chatbotDefaultConfiguration)
            ,Times.Once);
    }

    [Fact]
    public async void DeselectCurrentChatbotDefaultConfiguration()
    {
        var configurationRepositoryMock = new Mock<IConfigurationRepository>();
        var configurationService = new ConfigurationService(configurationRepositoryMock.Object);
        var chatbotDefaultConfiguration = new ChatbotDefaultConfigurationDto();
        chatbotDefaultConfiguration.ChatbotName = "ChatGPT";

        configurationRepositoryMock.Setup(repo => repo.GetSelectedChatbotDefaultConfiguration("id"))
            .ReturnsAsync(chatbotDefaultConfiguration);

        await configurationService.DeselectCurrentChatbotDefaultConfiguration("id");
        
        Assert.False(chatbotDefaultConfiguration.Selected);
        configurationRepositoryMock.Verify(repo => repo.EditChatbotDefaultConfiguration(chatbotDefaultConfiguration), 
            Times.Once);

        configurationRepositoryMock.Invocations.Clear();
        
        configurationRepositoryMock.Setup(repo => repo.GetSelectedChatbotDefaultConfiguration("id"))
            .ReturnsAsync((ChatbotDefaultConfigurationDto?)null);
        
        await configurationService.DeselectCurrentChatbotDefaultConfiguration("id");
        
        configurationRepositoryMock.Verify(repo => repo.EditChatbotDefaultConfiguration(chatbotDefaultConfiguration), 
            Times.Never);
    }

    [Fact]
    public void CreateChatSessionDefaultConfiguration()
    {
        var configurationRepositoryMock = new Mock<IConfigurationRepository>();
        var configurationService = new ConfigurationService(configurationRepositoryMock.Object);

        var result = configurationService.CreateChatSessionDefaultConfiguration();
        
        Assert.Equal("You are a helpful assistant. You can help me by answering my questions.", result.SystemInstruction);
        Assert.False(result.TextStream);
        Assert.Equal("1024x1024", result.ImageSize);
        Assert.Equal(1, result.ImagesToGenerate);
    }

    [Fact]
    public async void GetChatSessionDefaultConfiguration()
    {
        var configurationRepositoryMock = new Mock<IConfigurationRepository>();
        var configurationService = new ConfigurationService(configurationRepositoryMock.Object);
        var chatSessionDefaultConfiguration = new ChatSessionDefaultConfigurationDto();

        configurationRepositoryMock.Setup(repo => repo.GetChatSessionDefaultConfiguration("id"))
            .ReturnsAsync(chatSessionDefaultConfiguration);

        var result = await configurationService.GetChatSessionDefaultConfiguration("id");
        
        configurationRepositoryMock.Verify(repo => repo.GetChatSessionDefaultConfiguration("id"),
            Times.Once);
        Assert.Equal(chatSessionDefaultConfiguration, result);
    }

    [Fact]
    public async void SaveChatSessionDefaultConfiguration()
    {
        var configurationRepositoryMock = new Mock<IConfigurationRepository>();
        var configurationService = new ConfigurationService(configurationRepositoryMock.Object);
        var chatSessionDefaultConfiguration = new ChatSessionDefaultConfigurationDto();

        await configurationService.SaveChatSessionDefaultConfiguration(chatSessionDefaultConfiguration, "id");
        
        configurationRepositoryMock
            .Verify(repo => repo.SaveChatSessionDefaultConfiguration(chatSessionDefaultConfiguration, 
                "id"), Times.Once);
    }
    
    [Fact]
    public async void EditChatSessionDefaultConfiguration()
    {
        var configurationRepositoryMock = new Mock<IConfigurationRepository>();
        var configurationService = new ConfigurationService(configurationRepositoryMock.Object);
        var chatSessionDefaultConfiguration = new ChatSessionDefaultConfigurationDto();

        await configurationService.EditChatSessionDefaultConfiguration(chatSessionDefaultConfiguration);
        
        configurationRepositoryMock
            .Verify(repo => repo.EditChatSessionDefaultConfiguration(chatSessionDefaultConfiguration),
                Times.Once);
    }

    [Fact]
    public async void GetChatSessionConfiguration()
    {
        var configurationRepositoryMock = new Mock<IConfigurationRepository>();
        var configurationService = new ConfigurationService(configurationRepositoryMock.Object);
        var chatSessionConfiguration = new ChatSessionConfigurationDto();

        configurationRepositoryMock.Setup(repo => repo.GetChatSessionConfiguration("id"))
            .ReturnsAsync(chatSessionConfiguration);

        var result = await configurationService.GetChatSessionConfiguration("id");
        
        configurationRepositoryMock.Verify(repo => repo.GetChatSessionConfiguration("id"),
            Times.Once);
        Assert.Equal(chatSessionConfiguration, result);
    }

    [Fact]
    public async void SaveChatSessionConfiguration()
    {
        var configurationRepositoryMock = new Mock<IConfigurationRepository>();
        var configurationService = new ConfigurationService(configurationRepositoryMock.Object);
        var chatSessionConfiguration = new ChatSessionConfigurationDto();
        
        await configurationService.SaveChatSessionConfiguration(chatSessionConfiguration, "id");
        
        configurationRepositoryMock
            .Verify(repo => repo.SaveChatSessionConfiguration(chatSessionConfiguration, "id"),
                Times.Once);
    }
    
    [Fact]
    public async void EditChatSessionConfiguration()
    {
        var configurationRepositoryMock = new Mock<IConfigurationRepository>();
        var configurationService = new ConfigurationService(configurationRepositoryMock.Object);
        var chatSessionConfiguration = new ChatSessionConfigurationDto();
        
        await configurationService.EditChatSessionConfiguration(chatSessionConfiguration, "id");
        
        configurationRepositoryMock
            .Verify(repo => repo.EditChatSessionConfiguration(chatSessionConfiguration, "id"),
                Times.Once);
    }

    [Fact]
    public async void DeleteChatSessionAndItsConfiguration()
    {
        var configurationRepositoryMock = new Mock<IConfigurationRepository>();
        var configurationService = new ConfigurationService(configurationRepositoryMock.Object);
        var chatSessionConfiguration = new ChatSessionConfigurationDto();
        chatSessionConfiguration.Id = "id";
        
        configurationRepositoryMock.Setup(repo => repo.GetChatSessionConfiguration("id"))
            .ReturnsAsync(chatSessionConfiguration);

        await configurationService.DeleteChatSessionAndItsConfiguration("id");
        
        configurationRepositoryMock.Verify(repo => repo.DeleteChatSessionConfiguration("id"),
            Times.Once);

        configurationRepositoryMock.Invocations.Clear();
        
        chatSessionConfiguration.Id = null;
        
        configurationRepositoryMock.Setup(repo => repo.GetChatSessionConfiguration("id"))
            .ReturnsAsync(chatSessionConfiguration);
        
        configurationRepositoryMock.Verify(repo => repo.DeleteChatSessionConfiguration("id"),
            Times.Never);
        
        configurationRepositoryMock.Invocations.Clear();
        
        configurationRepositoryMock.Setup(repo => repo.GetChatSessionConfiguration("id"))
            .ReturnsAsync((ChatSessionConfigurationDto?)null);
        
        configurationRepositoryMock.Verify(repo => repo.DeleteChatSessionConfiguration("id"),
            Times.Never);
    }
}