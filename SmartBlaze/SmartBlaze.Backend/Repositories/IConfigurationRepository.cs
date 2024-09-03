using SmartBlaze.Backend.Dtos;

namespace SmartBlaze.Backend.Repositories;

public interface IConfigurationRepository
{
    Task<ChatbotDefaultConfigurationDto?> GetChatbotDefaultConfiguration(string chatbotName, string userId);
    Task<ChatbotDefaultConfigurationDto?> GetSelectedChatbotDefaultConfiguration(string userId);
    Task SaveChatbotDefaultConfiguration(ChatbotDefaultConfigurationDto chatbotDefaultConfigurationDto, string userId);
    Task EditChatbotDefaultConfiguration(ChatbotDefaultConfigurationDto chatbotDefaultConfigurationDto);
    Task<ChatSessionDefaultConfigurationDto?> GetChatSessionDefaultConfiguration(string userId);
    Task SaveChatSessionDefaultConfiguration(ChatSessionDefaultConfigurationDto chatSessionDefaultConfigurationDto,
        string userId);
    Task EditChatSessionDefaultConfiguration(ChatSessionDefaultConfigurationDto chatSessionDefaultConfigurationDto);
    Task<ChatSessionConfigurationDto?> GetChatSessionConfiguration(string chatSessionId);
    Task SaveChatSessionConfiguration(ChatSessionConfigurationDto chatSessionConfigurationDto, string chatSessionId);
    Task EditChatSessionConfiguration(ChatSessionConfigurationDto chatSessionConfigurationDto, string chatSessionId);
    Task DeleteChatSessionConfiguration(string id);
}