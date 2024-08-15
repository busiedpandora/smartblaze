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
    private ChatSessionService _chatSessionService;


    public ConfigurationController(ConfigurationService configurationService, ChatbotService chatbotService,
        ChatSessionService chatSessionService)
    {
        _configurationService = configurationService;
        _chatbotService = chatbotService;
        _chatSessionService = chatSessionService;
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
                await _configurationService.SaveChatbotDefaultConfiguration(chatbotDefaultConfiguration);
            }
            
            chatbotConfigurationDtos.Add(chatbotDefaultConfiguration);
        }

        return Ok(chatbotConfigurationDtos);
    }

    [HttpPost("chatbot")]
    public async Task<ActionResult> EditChatbotDefaultConfiguration([FromBody] ChatbotDefaultConfigurationDto chatbotDefaultConfigurationDto)
    {
        if (chatbotDefaultConfigurationDto.ChatbotName is null || chatbotDefaultConfigurationDto.TextGenerationChatbotModel is null
                                                               || chatbotDefaultConfigurationDto.ImageGenerationChatbotModel is null)
        {
            return BadRequest("The chatbot name and model cannot be null");
        }
        
        var chatbotDefaultConfiguration = await _configurationService.GetChatbotDefaultConfiguration(chatbotDefaultConfigurationDto.ChatbotName);

        if (chatbotDefaultConfiguration is null)
        {
            return NotFound($"Cannot find configuration for chatbot {chatbotDefaultConfigurationDto.ChatbotName}");
        }
        
        if (chatbotDefaultConfiguration.Selected == false)
        {
            await _configurationService.DeselectCurrentChatbotDefaultConfiguration();
        }

        chatbotDefaultConfiguration.TextGenerationChatbotModel = chatbotDefaultConfigurationDto.TextGenerationChatbotModel;
        chatbotDefaultConfiguration.ImageGenerationChatbotModel = chatbotDefaultConfigurationDto.ImageGenerationChatbotModel;
        chatbotDefaultConfiguration.ApiHost = chatbotDefaultConfigurationDto.ApiHost ?? "";
        chatbotDefaultConfiguration.ApiKey = chatbotDefaultConfigurationDto.ApiKey ?? "";
        chatbotDefaultConfiguration.Temperature = chatbotDefaultConfigurationDto.Temperature;
        chatbotDefaultConfiguration.Selected = true;

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
    public async Task<ActionResult> EditChatSessionDefaultConfiguration(
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

    [HttpGet("chat-session/{id}")]
    public async Task<ActionResult<ChatSessionConfigurationDto>> GetChatSessionConfiguration(string id)
    {
        ChatSessionDto? chatSessionDto = await _chatSessionService.GetChatSessionById(id);
        
        if (chatSessionDto is null)
        {
            return NotFound($"Chat session with id {id} not found");
        }

        var chatSessionConfiguration = await _configurationService.GetChatSessionConfiguration(id);

        return Ok(chatSessionConfiguration);
    }

    [HttpPost("chat-session/{id}")]
    public async Task<ActionResult> AddChatSessionConfiguration(string id, 
        [FromBody] ChatSessionConfigurationDto chatSessionConfigurationDto)
    {
        ChatSessionDto? chatSessionDto = await _chatSessionService.GetChatSessionById(id);
        
        if (chatSessionDto is null)
        {
            return NotFound($"Chat session with id {id} not found");
        }
        
        if (chatSessionConfigurationDto.ChatbotName is null || chatSessionConfigurationDto.TextGenerationChatbotModel is null 
                                                            || chatSessionConfigurationDto.ImageGenerationChatbotModel is null)
        {
            return BadRequest($"No chatbot specified for chat session with id {id}");
        }

        await _configurationService.SaveChatSessionConfiguration(chatSessionConfigurationDto, id);

        return Ok();
    }
}