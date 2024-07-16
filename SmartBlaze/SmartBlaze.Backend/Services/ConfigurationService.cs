using SmartBlaze.Backend.Dtos;
using SmartBlaze.Backend.Models;
using SmartBlaze.Backend.Repositories;

namespace SmartBlaze.Backend.Services;

public class ConfigurationService
{
    private ConfigurationRepository _configurationRepository;
    

    public ConfigurationService(ConfigurationRepository configurationRepository)
    {
        _configurationRepository = configurationRepository;
    }

    public async Task<ChatbotConfigurationDto?> GetChatbotConfiguration(string chatbotName)
    {
        return await _configurationRepository.GetChatbotConfiguration(chatbotName);
    }

    public async Task AddChatbotConfiguration(ChatbotConfigurationDto chatbotConfigurationDto)
    {
        await _configurationRepository.SaveChatbotConfiguration(chatbotConfigurationDto);
    }

    public async Task EditChatbotConfiguration(ChatbotConfigurationDto chatbotConfigurationDto)
    {
        await _configurationRepository.EditChatbotConfiguration(chatbotConfigurationDto);
    }

    public async Task UnselectCurrentChatbot()
    {
        var selectedChatbotConfiguration = await _configurationRepository.GetSelectedChatbotConfiguration();

        if (selectedChatbotConfiguration is not null)
        {
            selectedChatbotConfiguration.Selected = false;
            await _configurationRepository.EditChatbotConfiguration(selectedChatbotConfiguration);
        }
    }
    
    public ChatSessionConfigurationDto GetDefaultChatSessionConfiguration()
    {
        return new ChatSessionConfigurationDto()
        {
            SystemInstruction = "You are a helpful assistant. You can help me by answering my questions.",
            TextStream = true
        };
    }

    public async Task<ChatSessionConfigurationDto?> GetChatSessionConfiguration()
    {
        return await _configurationRepository.GetChatSessionConfiguration();
    }

    public async Task SaveChatSessionConfiguration(ChatSessionConfigurationDto chatSessionConfigurationDto)
    {
        await _configurationRepository.SaveChatSessionConfiguration(chatSessionConfigurationDto);
    }
    
    public async Task EditChatSessionConfiguration(ChatSessionConfigurationDto chatSessionConfigurationDto)
    {
        await _configurationRepository.EditChatSessionConfiguration(chatSessionConfigurationDto);
    }
}