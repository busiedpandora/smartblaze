using SmartBlaze.Backend.Dtos;
using SmartBlaze.Backend.Models;
using SmartBlaze.Backend.Services;

namespace SmartBlaze.Backend.Controllers;

public class ChatSessionController
{
    private ChatSessionService _chatSessionService;
    private MessageService _messageService;


    public ChatSessionController(ChatSessionService chatSessionService, MessageService messageService)
    {
        _chatSessionService = chatSessionService;
        _messageService = messageService;
    }
    
    public List<ChatSession> GetAllChatSessions()
    {
        return _chatSessionService.GetAllChatSessions()
            .OrderByDescending(cs => cs.CreationDate)
            .ToList();
    }
    
    public ChatSession? GetChatSessionById(long id)
    {
        return _chatSessionService.GetChatSessionById(id);
    }

    public Message CreateNewUserMessage(string content)
    {
        Message message = _messageService.CreateNewUserMessage(content);

        return message;
    }

    public Message CreateNewSystemMessage(string content)
    {
        Message message = _messageService.CreateNewSystemMessage(content);

        return message;
    }

    public async Task<Message?> GenerateNewAssistantMessage(ChatSession chatSession)
    {
        string? content = await UpdateChatSession(chatSession);
        
        if (!string.IsNullOrEmpty(content))
        {
            Message message = _messageService.CreateNewAssistantMessage(content);
            
            return message;
        }

        return null;
    }

    public void AddNewMessageToChatSession(Message message, ChatSession chatSession)
    {
        _chatSessionService.AddNewMessageToChatSession(message, chatSession);
    }

    public ChatSession CreateNewChatSession(string title)
    {
        ChatSession chatSession = _chatSessionService.CreateNewChatSession(title);

        return chatSession;
    }

    public void AddChatSession(ChatSession chatSession)
    {
        _chatSessionService.AddChatSession(chatSession);
    }

    private async Task<string?> UpdateChatSession(ChatSession chatSession)
    {
        ChatResponseDto? chatResponseDto = await _chatSessionService.GenerateChatResponse(chatSession);

        if (chatResponseDto is not null)
        {
            MessageDto? messageDto = _chatSessionService.GetMessageFromChatResponse(chatResponseDto);

            if (messageDto is not null)
            {
                return messageDto.Content;
            }
        }

        return string.Empty;
    }
}