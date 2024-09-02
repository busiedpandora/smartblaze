using SmartBlaze.Backend.Dtos;
using SmartBlaze.Backend.Models;
using SmartBlaze.Backend.Repositories;

namespace SmartBlaze.Backend.Services;

public class ChatSessionService : IChatSessionService
{
    private readonly IChatSessionRepository _chatSessionRepository;
    
    public ChatSessionService(IChatSessionRepository chatSessionRepository)
    {
        _chatSessionRepository = chatSessionRepository;
    }
    
    public async Task<List<ChatSessionDto>?> GetAllChatSessions(string userId)
    {
        var chatSessionDtos = await _chatSessionRepository.GetAllChatSessions(userId);

        return chatSessionDtos;
    }

    public async Task<ChatSessionDto?> GetChatSessionById(string id)
    {
        var chatSessionDto = await _chatSessionRepository.GetChatSessionById(id);

        return chatSessionDto;
    }

    public ChatSessionDto CreateNewChatSession(string title)
    {
        return new ChatSessionDto()
        {
            Title = title,
            CreationDate = DateTime.Now,
        };
    }

    public async Task<ChatSessionDto> AddChatSession(ChatSessionDto chatSessionDto, string userId)
    {
        var csDto = await _chatSessionRepository.SaveChatSession(chatSessionDto, userId);

        return csDto;
    }

    public async Task EditChatSession(ChatSessionDto chatSessionDto)
    {
        await _chatSessionRepository.EditChatSession(chatSessionDto);
    }
}