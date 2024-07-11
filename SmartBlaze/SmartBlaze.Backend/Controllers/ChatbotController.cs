using Microsoft.AspNetCore.Mvc;
using SmartBlaze.Backend.Dtos;
using SmartBlaze.Backend.Services;

namespace SmartBlaze.Backend.Controllers;

[ApiController]
[Route("chatbots")]
public class ChatbotController : ControllerBase
{
    private ChatbotService _chatbotService;


    public ChatbotController(ChatbotService chatbotService)
    {
        _chatbotService = chatbotService;
    }
    
    [HttpGet("")]
    public ActionResult<List<ChatbotDto>> GetAllChatbots()
    {
        var chatbots = _chatbotService.GetAllChatbots();

        if (chatbots is null)
        {
            return NotFound($"No chatbots available");
        }

        return Ok(chatbots
            .Select(c => new ChatbotDto()
        {
            Name = c.Name,
            Models = c.Models
        })
            .ToList());
    }
}