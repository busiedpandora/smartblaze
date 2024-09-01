using Microsoft.AspNetCore.Mvc;
using SmartBlaze.Backend.Dtos;
using SmartBlaze.Backend.Services;

namespace SmartBlaze.Backend.Controllers;

[ApiController]
[Route("configuration")]
public class ConfigurationController : ControllerBase
{
    private readonly ConfigurationService _configurationService;
    private readonly ChatbotService _chatbotService;
    private readonly ChatSessionService _chatSessionService;


    public ConfigurationController(ConfigurationService configurationService, ChatbotService chatbotService,
        ChatSessionService chatSessionService)
    {
        _configurationService = configurationService;
        _chatbotService = chatbotService;
        _chatSessionService = chatSessionService;
    }
    
    [HttpGet("{userId}/chatbot")]
    public async Task<ActionResult<List<ChatbotDefaultConfigurationDto>>> GetChatbotDefaultConfigurations(string userId)
    {
        var chatbots = _chatbotService.GetAllChatbots();
        var chatbotConfigurationDtos = new List<ChatbotDefaultConfigurationDto>();

        foreach (var chatbot in chatbots)
        {
            var chatbotDefaultConfiguration = await _configurationService.GetChatbotDefaultConfiguration(chatbot.Name, userId);

            if (chatbotDefaultConfiguration is null)
            {
                chatbotDefaultConfiguration = chatbot.GetDefaultConfiguration();
                await _configurationService.SaveChatbotDefaultConfiguration(chatbotDefaultConfiguration, userId);
            }
            
            chatbotConfigurationDtos.Add(chatbotDefaultConfiguration);
        }

        return Ok(chatbotConfigurationDtos);
    }

    [HttpPut("{userId}/chatbot")]
    public async Task<ActionResult> EditChatbotDefaultConfiguration(string userId,
        [FromBody] ChatbotDefaultConfigurationDto chatbotDefaultConfigurationDto)
    {
        if (chatbotDefaultConfigurationDto.ChatbotName is null)
        {
            return BadRequest("The chatbot name cannot be null");
        }
        
        var chatbotDefaultConfiguration = 
            await _configurationService.GetChatbotDefaultConfiguration(chatbotDefaultConfigurationDto.ChatbotName, userId);

        if (chatbotDefaultConfiguration is null)
        {
            return NotFound($"Cannot find configuration for chatbot {chatbotDefaultConfigurationDto.ChatbotName}");
        }
        
        if (chatbotDefaultConfiguration.Selected == false)
        {
            await _configurationService.DeselectCurrentChatbotDefaultConfiguration(userId);
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

    [HttpGet("{userId}/chat-session/")]
    public async Task<ActionResult<ChatSessionDefaultConfigurationDto>> GetChatSessionDefaultConfiguration(string userId)
    {
        var chatSessionDefaultConfiguration = await _configurationService.GetChatSessionDefaultConfiguration(userId);

        if (chatSessionDefaultConfiguration is null)
        {
            chatSessionDefaultConfiguration = _configurationService.CreateChatSessionDefaultConfiguration();
            await _configurationService.SaveChatSessionDefaultConfiguration(chatSessionDefaultConfiguration, userId);
        }

        return Ok(chatSessionDefaultConfiguration);
    }

    [HttpPut("{userId}/chat-session")]
    public async Task<ActionResult> EditChatSessionDefaultConfiguration(string userId,
        [FromBody] ChatSessionDefaultConfigurationDto chatSessionDefaultConfigurationDto)
    {
        if (chatSessionDefaultConfigurationDto.SystemInstruction is null)
        {
            return BadRequest(
                $"Properties of chat session configuration with id {chatSessionDefaultConfigurationDto.Id} specified incorrectly");
        }

        var chatSessionDefaultConfiguration = await _configurationService.GetChatSessionDefaultConfiguration(userId);

        if (chatSessionDefaultConfiguration is null)
        {
            return NotFound("Cannot find the chat session configuration");
        }
        
        chatSessionDefaultConfiguration.SystemInstruction = chatSessionDefaultConfigurationDto.SystemInstruction;
        chatSessionDefaultConfiguration.TextStream = chatSessionDefaultConfigurationDto.TextStream;
        chatSessionDefaultConfiguration.ImageSize = chatSessionDefaultConfiguration.ImageSize;
        chatSessionDefaultConfiguration.ImagesToGenerate = chatSessionDefaultConfiguration.ImagesToGenerate;

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