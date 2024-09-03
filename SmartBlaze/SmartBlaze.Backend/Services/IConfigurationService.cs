using SmartBlaze.Backend.Dtos;

namespace SmartBlaze.Backend.Services;

public interface IConfigurationService
{
    Task<ChatbotDefaultConfigurationDto?> GetChatbotDefaultConfiguration(string chatbotName, string userId);
    Task SaveChatbotDefaultConfiguration(ChatbotDefaultConfigurationDto chatbotDefaultConfigurationDto, string userId);
    Task EditChatbotDefaultConfiguration(ChatbotDefaultConfigurationDto chatbotDefaultConfigurationDto);
    Task DeselectCurrentChatbotDefaultConfiguration(string userId);
    ChatSessionDefaultConfigurationDto CreateChatSessionDefaultConfiguration();
    Task<ChatSessionDefaultConfigurationDto?> GetChatSessionDefaultConfiguration(string userId);

    Task SaveChatSessionDefaultConfiguration(ChatSessionDefaultConfigurationDto chatSessionDefaultConfigurationDto,
        string userId);

    Task EditChatSessionDefaultConfiguration(ChatSessionDefaultConfigurationDto chatSessionDefaultConfigurationDto);
    Task<ChatSessionConfigurationDto?> GetChatSessionConfiguration(string chatSessionId);
    Task SaveChatSessionConfiguration(ChatSessionConfigurationDto chatSessionConfigurationDto, string chatSessionId);
    Task EditChatSessionConfiguration(ChatSessionConfigurationDto chatSessionConfigurationDto, string chatSessionId);
    Task DeleteChatSessionAndItsConfiguration(string chatSessionId);
}