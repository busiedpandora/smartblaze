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
    private ConfigurationService _configurationService;


    public ChatSessionController(ChatSessionService chatSessionService, ConfigurationService configurationService)
    {
        _chatSessionService = chatSessionService;
        _configurationService = configurationService;
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
        if (chatSessionDto.Title is null)
        {
            return BadRequest("Chat session not specified correctly. Title must be provided");
        }
        
        chatSessionDto = _chatSessionService.CreateNewChatSession(chatSessionDto.Title);
        chatSessionDto = await _chatSessionService.AddChatSession(chatSessionDto);
        
        return Ok(chatSessionDto);
    }

    [HttpDelete("{id}/delete")]
    public async Task<ActionResult> DeleteChatSession(string id)
    {
        var chatSessionDto = await _chatSessionService.GetChatSessionById(id);

        if (chatSessionDto is null)
        {
            return NotFound($"Chat session with id {id} not found");
        }

        await _configurationService.DeleteChatSessionAndItsConfiguration(id);
        
        return Ok();
    }

    [HttpPut("{id}/edit")]
    public async Task<ActionResult> EditChatSession(string id, [FromBody] ChatSessionEditDto chatSessionEditDto)
    {
        ChatSessionDto? chatSessionDto = await _chatSessionService.GetChatSessionById(id);
        
        if (chatSessionDto is null)
        {
            return NotFound($"Chat session with id {id} not found");
        }
        
        if (chatSessionEditDto.Title is null)
        {
            return BadRequest("Chat session not specified correctly. Title must be provided");
        }

        if (chatSessionDto.Title != chatSessionEditDto.Title)
        {
            chatSessionDto.Title = chatSessionEditDto.Title;
            await _chatSessionService.EditChatSession(chatSessionDto);
        }

        if (chatSessionEditDto.ChatSessionConfigurationDto is not null)
        {
            var chatSessionConfiguration = await _configurationService.GetChatSessionConfiguration(id);

            if (chatSessionConfiguration is not null)
            {
                chatSessionEditDto.ChatSessionConfigurationDto.Id = chatSessionConfiguration.Id;
                await _configurationService.EditChatSessionConfiguration(chatSessionEditDto.ChatSessionConfigurationDto, id);
            }
        }

        return Ok();
    }
}