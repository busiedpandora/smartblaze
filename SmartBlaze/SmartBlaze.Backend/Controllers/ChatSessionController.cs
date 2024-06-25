using Microsoft.AspNetCore.Mvc;
using SmartBlaze.Backend.Dtos;
using SmartBlaze.Backend.Models;
using SmartBlaze.Backend.Services;

namespace SmartBlaze.Backend.Controllers;

[ApiController]
[Route("chatsessions")]
public class ChatSessionController : ControllerBase
{
    private ChatSessionService _chatSessionService;
    private MessageService _messageService;


    public ChatSessionController(ChatSessionService chatSessionService, MessageService messageService)
    {
        _chatSessionService = chatSessionService;
        _messageService = messageService;
    }

    [HttpGet("")]
    public ActionResult<List<ChatSessionDto>> GetAllChatSessions()
    {
        var chatSessions = _chatSessionService.GetAllChatSessions();

        if (chatSessions is null)
        {
            return NotFound();
        }
        
        return Ok(chatSessions
            .Select(ct => ChatSessionDto.ToChatSessionDto(ct.Id, ct.Title, ct.CreationDate, null))
            .ToList());
    }

    [HttpGet("{id}")]
    public ActionResult<ChatSessionDto> GetChatSession(long id)
    {
        var chatSession = _chatSessionService.GetChatSessionById(id);

        if (chatSession is null)
        {
            return NotFound();
        }

        return Ok(ChatSessionDto
            .ToChatSessionDto(chatSession.Id, chatSession.Title, chatSession.CreationDate, chatSession.Messages));
    }
    
    [HttpPost("new")]
    public ActionResult<ChatSessionDto> AddNewChatSession([FromBody] ChatSessionDto chatSessionDto)
    {
        if (chatSessionDto is null || chatSessionDto.Title is null)
        {
            return BadRequest();
        }

        ChatSession chatSession = _chatSessionService.CreateNewChatSession(chatSessionDto.Title);
        _chatSessionService.AddChatSession(chatSession);
        
        return Ok(ChatSessionDto.ToChatSessionDto(chatSession.Id, chatSession.Title, chatSession.CreationDate, null));
    }
    
    [HttpGet("{id}/messages")]
    public ActionResult<List<MessageDto>> GetMessagesFromChatSession(long id)
    {
        ChatSession? chatSession = _chatSessionService.GetChatSessionById(id);

        if (chatSession is null)
        {
            return NotFound();
        }

        if (chatSession.Messages is null)
        {
            return NoContent();
        }

        return chatSession.Messages
            .Select(m => MessageDto.ToMessageDto(m.Content, m.Role, m.CreationDate))
            .ToList();
    }
    
    [HttpPost("{id}/new-user-message")]
    public ActionResult<MessageDto> AddNewUserMessageToChatSession(long id, [FromBody] MessageDto messageDto)
    {
        if (messageDto is null || messageDto.Content is null)
        {
            return BadRequest();
        }

        ChatSession? chatSession = _chatSessionService.GetChatSessionById(id);
        if (chatSession is null)
        {
            return BadRequest();
        }

        Message message = _messageService.CreateNewUserMessage(messageDto.Content);
        _chatSessionService.AddNewMessageToChatSession(message, chatSession);

        return Ok(MessageDto.ToMessageDto(message.Content, message.Role, message.CreationDate));
    }
    
    [HttpPost("{id}/new-assistant-message")]
    public async Task<ActionResult<MessageDto>> GenerateNewAssistantMessageToChatSession(long id)
    {
        ChatSession? chatSession = _chatSessionService.GetChatSessionById(id);
        if (chatSession is null)
        {
            return BadRequest();
        }
        
        string? content = await _chatSessionService.GenerateAssistantMessageContentFromChatSession(chatSession);

        if (content is null)
        {
            return BadRequest();
        }

        Message assistantMessage = _messageService.CreateNewAssistantMessage(content);

        _chatSessionService.AddNewMessageToChatSession(assistantMessage, chatSession);

        return Ok(MessageDto.ToMessageDto(assistantMessage.Content, assistantMessage.Role, assistantMessage.CreationDate));
    }
}