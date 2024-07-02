using Microsoft.AspNetCore.Mvc;
using SmartBlaze.Backend.Dtos;
using SmartBlaze.Backend.Models;
using SmartBlaze.Backend.Services;

namespace SmartBlaze.Backend.Controllers;

[ApiController]
[Route("chat-sessions")]
public class ChatSessionController : ControllerBase
{
    private ChatSessionService _chatSessionService;
    private ChatbotService _chatbotService;


    public ChatSessionController(ChatSessionService chatSessionService, ChatbotService chatbotService)
    {
        _chatSessionService = chatSessionService;
        _chatbotService = chatbotService;
    }

    [HttpGet("")]
    public ActionResult<List<ChatSessionDto>> GetAllChatSessions()
    {
        var chatSessions = _chatSessionService.GetAllChatSessions();

        if (chatSessions is null)
        {
            return NotFound($"Chat sessions not found");
        }
        
        return Ok(chatSessions
            .Select(ct => ChatSessionDto.ToChatSessionDto(ct.Id, ct.Title, ct.CreationDate, null))
            .ToList());
    }

    [HttpGet("{id:long}")]
    public ActionResult<ChatSessionDto> GetChatSession(long id)
    {
        var chatSession = _chatSessionService.GetChatSessionById(id);

        if (chatSession is null)
        {
            return NotFound($"Chat session with id {id} not found");
        }

        return Ok(ChatSessionDto
            .ToChatSessionDto(chatSession.Id, chatSession.Title, chatSession.CreationDate, chatSession.Messages));
    }
    
    [HttpPost("new")]
    public ActionResult<ChatSessionDto> AddNewChatSession([FromBody] ChatSessionDto chatSessionDto)
    {
        if (chatSessionDto is null || chatSessionDto.Title is null)
        {
            return BadRequest("Chat session not specified correctly");
        }

        string chatbotName = "Google Gemini";

        Chatbot? chatbot = _chatbotService.GetChatbotByName(chatbotName);

        if (chatbot is null)
        {
            return NotFound($"Chatbot with name {chatbotName} not found");
        }
        
        ChatSession chatSession = _chatSessionService.CreateNewChatSession(chatSessionDto.Title, chatbot);
        _chatSessionService.AddChatSession(chatSession);
        
        return Ok(ChatSessionDto.ToChatSessionDto(chatSession.Id, chatSession.Title, chatSession.CreationDate, null));
    }
}