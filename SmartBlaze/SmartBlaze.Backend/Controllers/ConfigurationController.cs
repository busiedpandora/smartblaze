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
    public async Task<ActionResult<List<ChatbotConfigurationDto>>> GetChatbotConfiguration()
    {
        var chatbots = _chatbotService.GetAllChatbots();
        var chatbotConfigurationDtos = new List<ChatbotConfigurationDto>();

        foreach (var chatbot in chatbots)
        {
            var chatbotConfiguration = await _configurationService.GetChatbotConfiguration(chatbot.Name);

            if (chatbotConfiguration is null)
            {
                chatbotConfiguration = chatbot.GetDefaultConfiguration();
                await _configurationService.AddChatbotConfiguration(chatbotConfiguration);
            }
            
            var chatbotConfigurationDto = new ChatbotConfigurationDto()
            {
                ChatbotName = chatbot.Name,
                ChatbotModel = chatbotConfiguration.ChatbotModel,
                ApiHost = chatbotConfiguration.ApiHost,
                ApiKey = chatbotConfiguration.ApiKey,
                Selected = chatbotConfiguration.Selected,
                Models = chatbot.Models,
                TextStreamDelay = chatbotConfiguration.TextStreamDelay,
                Temperature = chatbotConfiguration.Temperature,
                MinTemperature = chatbotConfiguration.MinTemperature,
                MaxTemperature = chatbotConfiguration.MaxTemperature
            };
                
            chatbotConfigurationDtos.Add(chatbotConfigurationDto);
        }

        return Ok(chatbotConfigurationDtos);
    }

    [HttpPost("chatbot")]
    public async Task<ActionResult> UpdateChatbotConfiguration([FromBody] ChatbotConfigurationDto chatbotConfigurationDto)
    {
        if (chatbotConfigurationDto.ChatbotName is null || chatbotConfigurationDto.ChatbotModel is null)
        {
            return BadRequest("The chatbot name and model cannot be null");
        }
        
        var chatbotConfiguration = await _configurationService.GetChatbotConfiguration(chatbotConfigurationDto.ChatbotName);

        if (chatbotConfiguration is null)
        {
            return NotFound($"Cannot found configuration for chatbot {chatbotConfigurationDto.ChatbotName}");
        }

        if (chatbotConfiguration.Selected == false)
        {
            await _configurationService.DeselectCurrentChatbotConfiguration();
        }

        chatbotConfiguration.ChatbotModel = chatbotConfigurationDto.ChatbotModel;
        chatbotConfiguration.ApiHost = chatbotConfigurationDto.ApiHost ?? "";
        chatbotConfiguration.ApiKey = chatbotConfigurationDto.ApiKey ?? "";
        chatbotConfigurationDto.TextStreamDelay = chatbotConfigurationDto.TextStreamDelay;
        chatbotConfiguration.Selected = true;
        chatbotConfiguration.Temperature = chatbotConfigurationDto.Temperature;

        await _configurationService.EditChatbotConfiguration(chatbotConfiguration);

        return Ok();
    }

    [HttpGet("chat-session")]
    public async Task<ActionResult<ChatSessionConfigurationDto>> GetChatSessionConfiguration()
    {
        var chatSessionConfiguration = await _configurationService.GetChatSessionConfiguration();

        if (chatSessionConfiguration is null)
        {
            chatSessionConfiguration = _configurationService.GetDefaultChatSessionConfiguration();
            await _configurationService.SaveChatSessionConfiguration(chatSessionConfiguration);
        }

        return Ok(chatSessionConfiguration);
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