using SmartBlaze.Backend.Dtos;
using SmartBlaze.Backend.Models;
using SmartBlaze.Backend.Repositories;

namespace SmartBlaze.Backend.Services;

public class ChatSessionService
{
    private ChatSessionRepository _chatSessionRepository;
    
    public ChatSessionService(ChatSessionRepository chatSessionRepository)
    {
        _chatSessionRepository = chatSessionRepository;
    }
    
    public async Task<List<ChatSessionDto>?> GetAllChatSessions()
    {
        var chatSessionDtos = await _chatSessionRepository.GetAllChatSessions();

        return chatSessionDtos;
    }

    public async Task<ChatSessionDto?> GetChatSessionById(string id)
    {
        var chatSessionDto = await _chatSessionRepository.GetChatSessionById(id);

        return chatSessionDto;
    }

    public ChatSessionDto CreateNewChatSession(string title, Chatbot chatbot, string systemInstruction)
    {
        string model = chatbot.Models.ElementAt(0);
        
        return new ChatSessionDto()
        {
            Title = title,
            CreationDate = DateTime.Now,
            ChatbotName = chatbot.Name,
            ChatbotModel = model,
            SystemInstruction = systemInstruction
        };
    }

    public async Task<ChatSessionDto> AddChatSession(ChatSessionDto chatSessionDto)
    {
        var csDto = await _chatSessionRepository.SaveChatSession(chatSessionDto);

        return csDto;
    }

    public async Task<ChatSessionDto> EditChatSession(ChatSessionDto chatSessionDto)
    {
        var csDto = await _chatSessionRepository.SaveChatSession(chatSessionDto);
        
        return csDto;
    }
}