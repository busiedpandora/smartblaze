using Microsoft.AspNetCore.Mvc;
using SmartBlaze.Backend.Dtos;
using SmartBlaze.Backend.Models;
using SmartBlaze.Backend.Services;

namespace SmartBlaze.Backend.Controllers;

[ApiController]
[Route("chat-sessions")]
public class ChatSessionController : ControllerBase
{
    private readonly ChatSessionService _chatSessionService;
    private readonly ConfigurationService _configurationService;
    private readonly ChatbotService _chatbotService;


    public ChatSessionController(ChatSessionService chatSessionService, ConfigurationService configurationService,
        ChatbotService chatbotService)
    {
        _chatSessionService = chatSessionService;
        _configurationService = configurationService;
        _chatbotService = chatbotService;
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<List<ChatSessionDto>>> GetAllChatSessions(string userId)
    {
        var chatSessionDtos = await _chatSessionService.GetAllChatSessions(userId);

        if (chatSessionDtos is null)
        {
            return NotFound($"Chat sessions not found");
        }
        
        return Ok(chatSessionDtos);
    }

    /*[HttpGet("{id}")]
    public async Task<ActionResult<ChatSessionDto>> GetChatSession(string id)
    {
        var chatSessionDto = await _chatSessionService.GetChatSessionById(id);

        if (chatSessionDto is null)
        {
            return NotFound($"Chat session with id {id} not found");
        }

        return Ok(chatSessionDto);
    }*/
    
    [HttpPost("{userId}/new")]
    public async Task<ActionResult<ChatSessionDto>> AddNewChatSession(string userId, [FromBody] ChatSessionDto chatSessionDto)
    {
        if (chatSessionDto.Title is null)
        {
            return BadRequest("Chat session not specified correctly. Title must be provided");
        }
        
        chatSessionDto = _chatSessionService.CreateNewChatSession(chatSessionDto.Title);
        chatSessionDto = await _chatSessionService.AddChatSession(chatSessionDto, userId);
        
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

    [HttpPut("{id}/entitle")]
    public async Task<ActionResult<string>> EntitleChatSessionFromUserMessage(string id, 
        [FromBody] ChatSessionInfoDto chatSessionInfoDto)
    {
        ChatSessionDto? chatSessionDto = await _chatSessionService.GetChatSessionById(id);
        
        if (chatSessionDto is null)
        {
            return NotFound($"Chat session with id {id} not found");
        }
        
        if (chatSessionInfoDto.ChatbotName is null or "")
        {
            return NotFound($"Chat session with id {id} has no chatbot specified");
        }

        if (chatSessionInfoDto.ChatbotModel is null or "")
        {
            return BadRequest($"No model specified for chatbot {chatSessionInfoDto.ChatbotName}");
        }

        if (chatSessionInfoDto.LastUserMessage is null || chatSessionInfoDto.ApiHost is null or "" ||
            chatSessionInfoDto.ApiKey is null or "")
        {
            return BadRequest(
                $"Last user message, API host and the API key must be specified for the chat session with id {id} " +
                $"for generating the assistant message");
        }
        
        Chatbot? chatbot = _chatbotService.GetChatbotByName(chatSessionInfoDto.ChatbotName);
        
        if (chatbot is null)
        {
            return NotFound($"Chat session with id {id} has no chatbot specified");
        }
        
        var chatbotModel = chatbot.TextGenerationChatbotModels.Count == 0 ? null : chatbot.TextGenerationChatbotModels.ElementAt(0);

        if (chatbotModel is null)
        {
            return NotFound(
                $"No model with name {chatSessionInfoDto.ChatbotModel} found for chatbot {chatSessionInfoDto.ChatbotName}");
        }

        var textGenerationRequestData = new TextGenerationRequestData()
        {
            Messages = new List<MessageDto>() { chatSessionInfoDto.LastUserMessage },
            ChatbotModel = chatbotModel,
            ApiHost = chatSessionInfoDto.ApiHost,
            ApiKey = chatSessionInfoDto.ApiKey,
            SystemInstruction = "Summarize in maximum 2 words the user text content. " +
                                "Use only word characters and maximum a space character " +
                                "Don't use more than 20 chars for the summary and don't use new line or tab characters" +
                                "If the text contains questions, do not answer them. Don't use punctuation marks."
        };

        var assistantMessageInfo = await _chatbotService.EntitleChatSessionFromUserMessage(chatbot, textGenerationRequestData);

        if (assistantMessageInfo.Status == "ok")
        {
            chatSessionDto.Title = assistantMessageInfo.Text;
            await _chatSessionService.EditChatSession(chatSessionDto);
            
            return Ok(assistantMessageInfo.Text);
        }
        
        return Problem(assistantMessageInfo.Text);
    }
}