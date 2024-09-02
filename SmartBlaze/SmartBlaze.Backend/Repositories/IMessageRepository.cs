using SmartBlaze.Backend.Dtos;

namespace SmartBlaze.Backend.Repositories;

public interface IMessageRepository
{
    Task<List<MessageDto>> GetMessagesFromChatSession(string chatSessionId);
    Task SaveMessage(MessageDto messageDto, string chatSessionId);
}