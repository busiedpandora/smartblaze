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
    public async Task<ActionResult<List<ChatbotDefaultConfigurationDto>>> GetChatbotDefaultConfigurations()
    {
        var chatbots = _chatbotService.GetAllChatbots();
        var chatbotConfigurationDtos = new List<ChatbotDefaultConfigurationDto>();

        foreach (var chatbot in chatbots)
        {
            var chatbotDefaultConfiguration = await _configurationService.GetChatbotDefaultConfiguration(chatbot.Name);

            if (chatbotDefaultConfiguration is null)
            {
                chatbotDefaultConfiguration = chatbot.GetDefaultConfiguration();
                await _configurationService.AddChatbotDefaultConfiguration(chatbotDefaultConfiguration);
            }

            chatbotDefaultConfiguration.ChatbotModels = chatbot.Models;
                
            chatbotConfigurationDtos.Add(chatbotDefaultConfiguration);
        }

        return Ok(chatbotConfigurationDtos);
    }

    [HttpPost("chatbot")]
    public async Task<ActionResult> UpdateChatbotDefaultConfiguration([FromBody] ChatbotDefaultConfigurationDto chatbotDefaultConfigurationDto)
    {
        if (chatbotDefaultConfigurationDto.ChatbotName is null || chatbotDefaultConfigurationDto.ChatbotModel is null)
        {
            return BadRequest("The chatbot name and model cannot be null");
        }
        
        var chatbotDefaultConfiguration = await _configurationService.GetChatbotDefaultConfiguration(chatbotDefaultConfigurationDto.ChatbotName);

        if (chatbotDefaultConfiguration is null)
        {
            return NotFound($"Cannot found configuration for chatbot {chatbotDefaultConfigurationDto.ChatbotName}");
        }

        if (chatbotDefaultConfiguration.Selected == false)
        {
            await _configurationService.DeselectCurrentChatbotDefaultConfiguration();
        }

        chatbotDefaultConfiguration.ChatbotModel = chatbotDefaultConfigurationDto.ChatbotModel;
        chatbotDefaultConfiguration.ApiHost = chatbotDefaultConfigurationDto.ApiHost ?? "";
        chatbotDefaultConfiguration.ApiKey = chatbotDefaultConfigurationDto.ApiKey ?? "";
        chatbotDefaultConfigurationDto.TextStreamDelay = chatbotDefaultConfigurationDto.TextStreamDelay;
        chatbotDefaultConfiguration.Selected = true;
        chatbotDefaultConfiguration.Temperature = chatbotDefaultConfigurationDto.Temperature;

        await _configurationService.EditChatbotDefaultConfiguration(chatbotDefaultConfiguration);

        return Ok();
    }

    [HttpGet("chat-session")]
    public async Task<ActionResult<ChatSessionDefaultConfigurationDto>> GetChatSessionDefaultConfiguration()
    {
        var chatSessionDefaultConfiguration = await _configurationService.GetChatSessionDefaultConfiguration();

        if (chatSessionDefaultConfiguration is null)
        {
            chatSessionDefaultConfiguration = _configurationService.CreateChatSessionDefaultConfiguration();
            await _configurationService.SaveChatSessionDefaultConfiguration(chatSessionDefaultConfiguration);
        }

        return Ok(chatSessionDefaultConfiguration);
    }

    [HttpPost("chat-session")]
    public async Task<ActionResult> UpdateChatSessionDefaultConfiguration(
        [FromBody] ChatSessionDefaultConfigurationDto chatSessionDefaultConfigurationDto)
    {
        if (chatSessionDefaultConfigurationDto.SystemInstruction is null)
        {
            return BadRequest(
                $"Properties of chat session configuration with id {chatSessionDefaultConfigurationDto.Id} specified incorrectly");
        }

        var chatSessionDefaultConfiguration = await _configurationService.GetChatSessionDefaultConfiguration();

        if (chatSessionDefaultConfiguration is null)
        {
            return NotFound("Cannot find the chat session configuration");
        }
        
        chatSessionDefaultConfiguration.SystemInstruction = chatSessionDefaultConfigurationDto.SystemInstruction;
        chatSessionDefaultConfiguration.TextStream = chatSessionDefaultConfigurationDto.TextStream;

        await _configurationService.EditChatSessionDefaultConfiguration(chatSessionDefaultConfiguration);

        return Ok();
    }
}