using SmartBlaze.Backend.Dtos;

namespace SmartBlaze.Backend.Repositories;

public interface IChatSessionRepository
{
    Task<List<ChatSessionDto>> GetAllChatSessions(string userId);
    Task<ChatSessionDto> GetChatSessionById(string id);
    Task<ChatSessionDto> SaveChatSession(ChatSessionDto chatSessionDto, string userId);
    Task EditChatSession(ChatSessionDto chatSessionDto);
}