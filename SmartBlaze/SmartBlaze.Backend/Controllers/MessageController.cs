using Microsoft.AspNetCore.Mvc;
using SmartBlaze.Backend.Dtos;
using SmartBlaze.Backend.Models;
using SmartBlaze.Backend.Services;

namespace SmartBlaze.Backend.Controllers;

[ApiController]
[Route("chat-session/{id}")]
public class MessageController : ControllerBase
{
    private ChatSessionService _chatSessionService;
    private MessageService _messageService;
    private ChatbotService _chatbotService;


    public MessageController(ChatSessionService chatSessionService, MessageService messageService, 
        ChatbotService chatbotService)
    {
        _chatSessionService = chatSessionService;
        _messageService = messageService;
        _chatbotService = chatbotService;
    }
    
    [HttpGet("messages")]
    public ActionResult<List<MessageDto>> GetMessagesFromChatSession(string id)
    {
        ChatSessionDto? chatSessionDtos = _chatSessionService.GetChatSessionById(id);

        if (chatSessionDtos is null)
        {
            return NotFound($"Chat session with id {id} not found");
        }

        if (chatSessionDtos.Messages is null)
        {
            return NoContent();
        }

        return chatSessionDtos.Messages;
    }
    
    [HttpPost("new-user-message")]
    public ActionResult<MessageDto> AddNewUserMessageToChatSession(string id, [FromBody] MessageDto messageDto)
    {
        if (messageDto is null || messageDto.Content is null)
        {
            return BadRequest("Message not specified correctly");
        }

        ChatSessionDto? chatSessionDto = _chatSessionService.GetChatSessionById(id);
        if (chatSessionDto is null)
        {
            return NotFound($"Chat session with id {id} not found");
        }

        MessageDto userMessageDto = _messageService.CreateNewUserMessage(messageDto.Content);
        _chatSessionService.AddNewMessageToChatSession(userMessageDto, chatSessionDto);

        return Ok(userMessageDto);
    }
    
    [HttpPost("new-assistant-message")]
    public async Task<ActionResult<MessageDto>> GenerateNewAssistantMessageToChatSession(string id)
    {
        ChatSessionDto? chatSessionDto = _chatSessionService.GetChatSessionById(id);
        if (chatSessionDto is null)
        {
            return NotFound($"Chat session with id {id} not found");
        }

        if (chatSessionDto.ChatbotName is null)
        {
            return NotFound($"Chat session with id {id} has no chatbot specified");
        }

        Chatbot? chatbot = _chatbotService.GetChatbotByName(chatSessionDto.ChatbotName);
        
        if (chatbot is null)
        {
            return NotFound($"Chat session with id {id} has no chatbot specified");
        }
        
        string? content = await _chatbotService.GenerateAssistantMessageContentFromChatSession(chatbot, chatSessionDto);
        
        if (content is null)
        {
            return Problem("Problem occured while generating the assistant message");
        }

        MessageDto assistantMessageDto = _messageService.CreateNewAssistantMessage(content);
        _chatSessionService.AddNewMessageToChatSession(assistantMessageDto, chatSessionDto);

        return Ok(assistantMessageDto);
    }

    [HttpPost("new-system-message")]
    public ActionResult<MessageDto> AddNewSystemMessageToChatSession(string id, [FromBody] MessageDto messageDto)
    {
        if (messageDto is null || messageDto.Content is null)
        {
            return BadRequest("Message not specified correctly");
        }
        
        ChatSessionDto? chatSessionDto = _chatSessionService.GetChatSessionById(id);
        if (chatSessionDto is null)
        {
            return NotFound($"Chat session with id {id} not found");
        }

        MessageDto systemMessageDto = _messageService.CreateNewSystemMessage(messageDto.Content);
        _chatSessionService.AddNewMessageToChatSession(systemMessageDto, chatSessionDto);

        return Ok(systemMessageDto);
    }
}