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

    public async Task<ChatbotDefaultConfigurationDto?> GetChatbotDefaultConfiguration(string chatbotName)
    {
        return await _configurationRepository.GetChatbotDefaultConfiguration(chatbotName);
    }
    
    public async Task AddChatbotDefaultConfiguration(ChatbotDefaultConfigurationDto chatbotDefaultConfigurationDto)
    {
        await _configurationRepository.SaveChatbotDefaultConfiguration(chatbotDefaultConfigurationDto);
    }
    
    public async Task EditChatbotDefaultConfiguration(ChatbotDefaultConfigurationDto chatbotDefaultConfigurationDto)
    {
        await _configurationRepository.EditChatbotDefaultConfiguration(chatbotDefaultConfigurationDto);
    }

    public async Task DeselectCurrentChatbotDefaultConfiguration()
    {
        var selectedChatbotDefaultConfiguration = await _configurationRepository.GetSelectedChatbotDefaultConfiguration();

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
            TextStream = true
        };
    }
    
    public async Task<ChatSessionDefaultConfigurationDto?> GetChatSessionDefaultConfiguration()
    {
        return await _configurationRepository.GetChatSessionDefaultConfiguration();
    }
    
    public async Task SaveChatSessionDefaultConfiguration(ChatSessionDefaultConfigurationDto chatSessionDefaultConfigurationDto)
    {
        await _configurationRepository.SaveChatSessionDefaultConfiguration(chatSessionDefaultConfigurationDto);
    }
    
    public async Task EditChatSessionDefaultConfiguration(ChatSessionDefaultConfigurationDto chatSessionDefaultConfigurationDto)
    {
        await _configurationRepository.EditChatSessionDefaultConfiguration(chatSessionDefaultConfigurationDto);
    }
}