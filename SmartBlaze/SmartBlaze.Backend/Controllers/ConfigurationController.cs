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

    [HttpGet("default/chat-session")]
    public ActionResult GetDefaultChatSessionConfigurations()
    {
        var chatSessionConfiguration = _configurationService.GetDefaultChatSessionConfiguration();

        return Ok(chatSessionConfiguration);
    }

    [HttpGet("model")]
    public ActionResult GetModelConfiguration()
    {
        return Ok();
    }

    [HttpGet("chat-session")]
    public ActionResult GetChatSessionConfiguration()
    {
        return Ok();
    }

    [HttpGet("chatbot")]
    public async Task<ActionResult> GetChatbotsConfiguration()
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
    public async Task<ActionResult> ConfigureChatbot([FromBody] ChatbotDto chatbotDto)
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
}