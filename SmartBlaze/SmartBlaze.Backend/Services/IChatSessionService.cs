using SmartBlaze.Backend.Dtos;

namespace SmartBlaze.Backend.Services;

public interface IChatSessionService
{
    Task<List<ChatSessionDto>?> GetAllChatSessions(string userId);
    Task<ChatSessionDto?> GetChatSessionById(string id);
    ChatSessionDto CreateNewChatSession(string title);
    Task<ChatSessionDto> AddChatSession(ChatSessionDto chatSessionDto, string userId);
    Task EditChatSession(ChatSessionDto chatSessionDto);
}