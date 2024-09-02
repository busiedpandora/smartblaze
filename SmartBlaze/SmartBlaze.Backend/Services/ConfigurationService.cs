using SmartBlaze.Backend.Dtos;
using SmartBlaze.Backend.Models;
using SmartBlaze.Backend.Repositories;

namespace SmartBlaze.Backend.Services;

public class ConfigurationService : IConfigurationService
{
    private readonly IConfigurationRepository _configurationRepository;
    

    public ConfigurationService(IConfigurationRepository configurationRepository)
    {
        _configurationRepository = configurationRepository;
    }

    public async Task<ChatbotDefaultConfigurationDto?> GetChatbotDefaultConfiguration(string chatbotName, string userId)
    {
        return await _configurationRepository.GetChatbotDefaultConfiguration(chatbotName, userId);
    }
    
    public async Task SaveChatbotDefaultConfiguration(ChatbotDefaultConfigurationDto chatbotDefaultConfigurationDto, string userId)
    {
        await _configurationRepository.SaveChatbotDefaultConfiguration(chatbotDefaultConfigurationDto, userId);
    }
    
    public async Task EditChatbotDefaultConfiguration(ChatbotDefaultConfigurationDto chatbotDefaultConfigurationDto)
    {
        await _configurationRepository.EditChatbotDefaultConfiguration(chatbotDefaultConfigurationDto);
    }

    public async Task DeselectCurrentChatbotDefaultConfiguration(string userId)
    {
        var selectedChatbotDefaultConfiguration = await _configurationRepository.GetSelectedChatbotDefaultConfiguration(userId);

        if (selectedChatbotDefaultConfiguration is not null)
        {
            selectedChatbotDefaultConfiguration.Selected = false;
            await _configurationRepository.EditChatbotDefaultConfiguration(selectedChatbotDefaultConfiguration);
        }
    }
    
    public ChatSessionDefaultConfigurationDto CreateChatSessionDefaultConfiguration()
    {
        return new ChatSessionDefaultConfigurationDto()
        {
            SystemInstruction = "You are a helpful assistant. You can help me by answering my questions.",
            TextStream = false,
            ImageSize = "1024x1024",
            ImagesToGenerate = 1
        };
    }
    
    public async Task<ChatSessionDefaultConfigurationDto?> GetChatSessionDefaultConfiguration(string userId)
    {
        return await _configurationRepository.GetChatSessionDefaultConfiguration(userId);
    }
    
    public async Task SaveChatSessionDefaultConfiguration(ChatSessionDefaultConfigurationDto chatSessionDefaultConfigurationDto,
        string userId)
    {
        await _configurationRepository.SaveChatSessionDefaultConfiguration(chatSessionDefaultConfigurationDto, userId);
    }
    
    public async Task EditChatSessionDefaultConfiguration(ChatSessionDefaultConfigurationDto chatSessionDefaultConfigurationDto)
    {
        await _configurationRepository.EditChatSessionDefaultConfiguration(chatSessionDefaultConfigurationDto);
    }

    public async Task<ChatSessionConfigurationDto?> GetChatSessionConfiguration(string chatSessionId)
    {
        return await _configurationRepository.GetChatSessionConfiguration(chatSessionId);
    }

    public async Task SaveChatSessionConfiguration(ChatSessionConfigurationDto chatSessionConfigurationDto, string chatSessionId)
    {
        await _configurationRepository.SaveChatSessionConfiguration(chatSessionConfigurationDto, chatSessionId);
    }

    public async Task EditChatSessionConfiguration(ChatSessionConfigurationDto chatSessionConfigurationDto, string chatSessionId)
    {
        await _configurationRepository.EditChatSessionConfiguration(chatSessionConfigurationDto, chatSessionId);
    }

    public async Task DeleteChatSessionAndItsConfiguration(string chatSessionId)
    {
        var chatSessionConfiguration = await GetChatSessionConfiguration(chatSessionId);

        if (chatSessionConfiguration is not null && chatSessionConfiguration.Id is not null)
        {
            await _configurationRepository.DeleteChatSessionConfiguration(chatSessionConfiguration.Id);
        }
    }
}