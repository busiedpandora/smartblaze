using Microsoft.AspNetCore.Mvc;
using SmartBlaze.Backend.Dtos;
using SmartBlaze.Backend.Models;
using SmartBlaze.Backend.Services;

namespace SmartBlaze.Backend.Controllers;


[ApiController]
[Route("chat-session/{id:long}")]
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
    public ActionResult<List<MessageDto>> GetMessagesFromChatSession(long id)
    {
        ChatSession? chatSession = _chatSessionService.GetChatSessionById(id);

        if (chatSession is null)
        {
            return NotFound($"Chat session with id {id} not found");
        }

        if (chatSession.Messages is null)
        {
            return NoContent();
        }

        return chatSession.Messages
            .Select(m => MessageDto.ToMessageDto(m.Content, m.Role, m.CreationDate))
            .ToList();
    }
    
    [HttpPost("new-user-message")]
    public ActionResult<MessageDto> AddNewUserMessageToChatSession(long id, [FromBody] MessageDto messageDto)
    {
        if (messageDto is null || messageDto.Content is null)
        {
            return BadRequest("Message not specified correctly");
        }

        ChatSession? chatSession = _chatSessionService.GetChatSessionById(id);
        if (chatSession is null)
        {
            return NotFound($"Chat session with id {id} not found");
        }

        Message message = _messageService.CreateNewUserMessage(messageDto.Content);
        _chatSessionService.AddNewMessageToChatSession(message, chatSession);

        return Ok(MessageDto.ToMessageDto(message.Content, message.Role, message.CreationDate));
    }
    
    [HttpPost("new-assistant-message")]
    public async Task<ActionResult<MessageDto>> GenerateNewAssistantMessageToChatSession(long id)
    {
        ChatSession? chatSession = _chatSessionService.GetChatSessionById(id);
        if (chatSession is null)
        {
            return NotFound($"Chat session with id {id} not found");
        }
        
        string? content = await _chatbotService.GenerateAssistantMessageContentFromChatSession(chatSession);
        
        if (content is null)
        {
            return Problem("Problem occured while generating the assistant message");
        }

        Message assistantMessage = _messageService.CreateNewAssistantMessage(content);
        _chatSessionService.AddNewMessageToChatSession(assistantMessage, chatSession);

        return Ok(MessageDto.ToMessageDto(assistantMessage.Content, assistantMessage.Role, assistantMessage.CreationDate));
    }

    [HttpPost("new-system-message")]
    public ActionResult<MessageDto> AddNewSystemMessageToChatSession(long id, [FromBody] MessageDto messageDto)
    {
        if (messageDto is null || messageDto.Content is null)
        {
            return BadRequest("Message not specified correctly");
        }
        
        ChatSession? chatSession = _chatSessionService.GetChatSessionById(id);
        if (chatSession is null)
        {
            return NotFound($"Chat session with id {id} not found");
        }

        Message systemMessage = _messageService.CreateNewSystemMessage(messageDto.Content);
        _chatSessionService.AddNewMessageToChatSession(systemMessage, chatSession);

        return Ok(MessageDto.ToMessageDto(systemMessage.Content, systemMessage.Role, systemMessage.CreationDate));
    }
}