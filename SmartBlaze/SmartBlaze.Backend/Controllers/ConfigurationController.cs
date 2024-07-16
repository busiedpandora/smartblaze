using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SmartBlaze.Backend.Dtos;
using SmartBlaze.Backend.Services;

namespace SmartBlaze.Backend.Controllers;

[ApiController]
[Route("configuration")]
public class ConfigurationController : ControllerBase
{
    private ConfigurationService _configurationService;
    private ChatbotService _chatbotService;


    public ConfigurationController(ConfigurationService configurationService, ChatbotService chatbotService)
    {
        _configurationService = configurationService;
        _chatbotService = chatbotService;
    }

    [HttpGet("chatbot")]
    public async Task<ActionResult<List<ChatbotDto>>> GetChatbotsConfiguration()
    {
        var chatbots = _chatbotService.GetAllChatbots();
        var chatbotDtos = new List<ChatbotDto>();

        foreach (var chatbot in chatbots)
        {
            var chatbotConfiguration = await _configurationService.GetChatbotConfiguration(chatbot.Name);

            if (chatbotConfiguration is not null)
            {
                var chatbotDto = new ChatbotDto()
                {
                    Name = chatbot.Name,
                    Model = chatbotConfiguration.ChatbotModel,
                    ApiHost = chatbotConfiguration.ApiHost,
                    ApiKey = chatbotConfiguration.ApiKey,
                    Selected = chatbotConfiguration.Selected,
                    Models = chatbot.Models
                };
                
                chatbotDtos.Add(chatbotDto);
            }
        }

        return Ok(chatbotDtos);
    }

    [HttpPost("chatbot/default")]
    public async Task<ActionResult> SetUpDefaultChatbotConfiguration()
    {
        _chatbotService.CreateChatbots();
        
        var chatbots = _chatbotService.GetAllChatbots();

        foreach (var chatbot in chatbots)
        {
            if (await _configurationService.GetChatbotConfiguration(chatbot.Name) is null)
            {
                var chatbotDefaultConfiguration = chatbot.GetDefaultConfiguration();
                await _configurationService.AddChatbotConfiguration(chatbotDefaultConfiguration);
            }
        }
        
        return Ok();
    }

    [HttpPost("chatbot")]
    public async Task<ActionResult> UpdateChatbotConfiguration([FromBody] ChatbotDto chatbotDto)
    {
        if (chatbotDto.Name is null || chatbotDto.Model is null)
        {
            return BadRequest("The chatbot name and model cannot be null");
        }
        
        var chatbotConfigurationDto = await _configurationService.GetChatbotConfiguration(chatbotDto.Name);

        if (chatbotConfigurationDto is null)
        {
            return NotFound($"Cannot found configuration for chatbot {chatbotDto.Name}");
        }

        if (chatbotConfigurationDto.Selected == false)
        {
            await _configurationService.UnselectCurrentChatbot();
        }

        chatbotConfigurationDto.ChatbotModel = chatbotDto.Model;
        chatbotConfigurationDto.ApiHost = chatbotDto.ApiHost ?? "";
        chatbotConfigurationDto.ApiKey = chatbotDto.ApiKey ?? "";
        chatbotConfigurationDto.Selected = true;

        await _configurationService.EditChatbotConfiguration(chatbotConfigurationDto);

        return Ok();
    }

    [HttpGet("chat-session")]
    public async Task<ActionResult<ChatSessionConfigurationDto>> GetChatSessionConfiguration()
    {
        var chatSessionConfiguration = await _configurationService.GetChatSessionConfiguration();

        return Ok(chatSessionConfiguration);
    }
    
    [HttpPost("chat-session/default")]
    public async Task<ActionResult> SetUpDefaultChatSessionConfiguration()
    {
        if (await _configurationService.GetChatSessionConfiguration() is null)
        {
            var chatSessionConfiguration = _configurationService.GetDefaultChatSessionConfiguration();

            await _configurationService.SaveChatSessionConfiguration(chatSessionConfiguration);
        }

        return Ok();
    }

    [HttpPost("chat-session")]
    public async Task<ActionResult> UpdateChatSessionConfiguration(
        [FromBody] ChatSessionConfigurationDto chatSessionConfigurationDto)
    {
        if (chatSessionConfigurationDto.SystemInstruction is null || chatSessionConfigurationDto.TextStream is null)
        {
            return BadRequest(
                $"Properties of chat session configuration with id {chatSessionConfigurationDto.Id} specified incorrectly");
        }

        var chatSessionConfiguration = await _configurationService.GetChatSessionConfiguration();

        if (chatSessionConfiguration is null)
        {
            return NotFound("Cannot find the chat session configuration");
        }
        
        chatSessionConfiguration.SystemInstruction = chatSessionConfigurationDto.SystemInstruction;
        chatSessionConfiguration.TextStream = chatSessionConfigurationDto.TextStream;

        await _configurationService.EditChatSessionConfiguration(chatSessionConfiguration);

        return Ok();
    }
}