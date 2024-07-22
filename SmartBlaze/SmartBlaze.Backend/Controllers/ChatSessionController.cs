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


    public ChatSessionController(ChatSessionService chatSessionService)
    {
        _chatSessionService = chatSessionService;
    }

    [HttpGet("")]
    public async Task<ActionResult<List<ChatSessionDto>>> GetAllChatSessions()
    {
        var chatSessionDtos = await _chatSessionService.GetAllChatSessions();

        if (chatSessionDtos is null)
        {
            return NotFound($"Chat sessions not found");
        }
        
        return Ok(chatSessionDtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ChatSessionDto>> GetChatSession(string id)
    {
        var chatSessionDto = await _chatSessionService.GetChatSessionById(id);

        if (chatSessionDto is null)
        {
            return NotFound($"Chat session with id {id} not found");
        }

        return Ok(chatSessionDto);
    }
    
    [HttpPost("new")]
    public async Task<ActionResult<ChatSessionDto>> AddNewChatSession([FromBody] ChatSessionDto chatSessionDto)
    {
        if (chatSessionDto is null || chatSessionDto.Title is null)
        {
            return BadRequest("Chat session not specified correctly");
        }
        
        chatSessionDto = _chatSessionService.CreateNewChatSession(chatSessionDto.Title);
        chatSessionDto = await _chatSessionService.AddChatSession(chatSessionDto);
        
        return Ok(chatSessionDto);
    }
}