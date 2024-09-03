using System.Text;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SmartBlaze.Backend.Dtos;
using SmartBlaze.Backend.Models;
using SmartBlaze.Backend.Services;

namespace SmartBlaze.Backend.Controllers;

[ApiController]
[Route("chat-session/{id}")]
public class MessageController : ControllerBase
{
    private readonly IChatSessionService _chatSessionService;
    private readonly IMessageService _messageService;
    private readonly IChatbotService _chatbotService;


    public MessageController(IChatSessionService chatSessionService, IMessageService messageService, 
        IChatbotService chatbotService)
    {
        _chatSessionService = chatSessionService;
        _messageService = messageService;
        _chatbotService = chatbotService;
    }
    
    [HttpGet("messages")]
    public async Task<ActionResult<List<MessageDto>>> GetMessagesFromChatSession(string id)
    {
        try
        {
            ChatSessionDto? chatSessionDto = await _chatSessionService.GetChatSessionById(id);

            if (chatSessionDto is null)
            {
                return NotFound($"Chat session with id {id} not found");
            }

            var messages = await _messageService.GetMessagesFromChatSession(chatSessionDto);

            return Ok(messages);
        }
        catch (Exception e)
        {
            return StatusCode(500, $"Request Error: {e.Message}");
        }
    }
    
    [HttpPost("new-user-message")]
    public async Task<ActionResult<MessageDto>> AddNewUserMessageToChatSession(string id, [FromBody] MessageDto messageDto)
    {
        if (messageDto.Text is null or "")
        {
            return BadRequest("Message not specified correctly");
        }

        ChatSessionDto? chatSessionDto = await _chatSessionService.GetChatSessionById(id);
        if (chatSessionDto is null)
        {
            return NotFound($"Chat session with id {id} not found");
        }

        MessageDto userMessageDto = _messageService.CreateNewUserMessage(messageDto.Text, messageDto.MediaDtos);
        try
        {
            await _messageService.AddNewMessageToChatSession(userMessageDto, chatSessionDto);

            return Ok(userMessageDto);
        }
        catch (Exception e)
        {
            return StatusCode(500, $"Request Error: {e.Message}");
        }
    }
    
    [HttpPost("new-assistant-message")]
    public async Task<ActionResult<MessageDto>> GenerateNewAssistantTextMessageInChatSession(string id, 
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

        if (chatSessionInfoDto.Messages is null || chatSessionInfoDto.ApiHost is null or "" ||
            chatSessionInfoDto.ApiKey is null or "")
        {
            return BadRequest(
                $"messages, API host and the API key must be specified for the chat session with id {id} " +
                $"for generating the assistant message");
        }

        Chatbot? chatbot = _chatbotService.GetChatbotByName(chatSessionInfoDto.ChatbotName);
        
        if (chatbot is null)
        {
            return NotFound($"Chat session with id {id} has no chatbot specified");
        }

        var chatbotModel = chatbot.TextGenerationChatbotModels
            .Find(tgm => tgm.Name == chatSessionInfoDto.ChatbotModel);

        if (chatbotModel is null)
        {
            return NotFound(
                $"No model with name {chatSessionInfoDto.ChatbotModel} found for chatbot {chatSessionInfoDto.ChatbotName}");
        }

        var textGenerationRequestData = new TextGenerationRequestData
        {
            Messages = chatSessionInfoDto.Messages,
            ChatbotModel = chatbotModel,
            ApiHost = chatSessionInfoDto.ApiHost,
            ApiKey = chatSessionInfoDto.ApiKey,
            SystemInstruction = chatSessionInfoDto.SystemInstruction,
            Temperature = chatSessionInfoDto.Temperature
        };
        
        var assistantMessageInfo = await _chatbotService.GenerateTextInChatSession(chatbot, textGenerationRequestData);

        MessageDto assistantMessageDto = _messageService.CreateNewAssistantTextMessage(assistantMessageInfo.Text ?? "", 
            chatSessionInfoDto.ChatbotName, chatSessionInfoDto.ChatbotModel, assistantMessageInfo.Status);
        try
        {
            await _messageService.AddNewMessageToChatSession(assistantMessageDto, chatSessionDto);
        }
        catch (Exception e)
        {
            return StatusCode(500, $"Request Error: {e.Message}");
        }

        return Ok(assistantMessageDto);
    }
    
    [HttpPost("new-assistant-empty-message")]
    public ActionResult<MessageDto> GetNewAssistantMessageWithEmptyContent(string id, [FromBody] ChatSessionInfoDto chatSessionInfoDto)
    {
        if (chatSessionInfoDto.ChatbotName is null or "")
        {
            return NotFound($"Chat session with id {id} has no chatbot specified");
        }

        if (chatSessionInfoDto.ChatbotModel is null or "")
        {
            return BadRequest($"No model specified for chatbot {chatSessionInfoDto.ChatbotName}");
        }
        
        MessageDto assistantMessageDto = _messageService.CreateNewAssistantTextMessage("", 
            chatSessionInfoDto.ChatbotName, chatSessionInfoDto.ChatbotModel, "ok");

        return Ok(assistantMessageDto);
    }
    
    [HttpPost("generate-assistant-stream-message")]
    public async IAsyncEnumerable<string?> GenerateNewAssistantTextMessageInChatSessionStreamEnabled(string id, 
        [FromBody] ChatSessionInfoDto chatSessionInfoDto)
    {
        ChatSessionDto? chatSessionDto = await _chatSessionService.GetChatSessionById(id);
        if (chatSessionDto is null)
        {
            var result = NotFound($"Chat session with id {id} not found");
            var responseDetails = new
            {
                StatusCode = result.StatusCode,
                Message = result.Value,
                Type = result.GetType().Name,
            };

            var fullResponse = JsonConvert.SerializeObject(responseDetails);
            yield return fullResponse;
            yield break;
        }

        if (chatSessionInfoDto.ChatbotName is null or "")
        {
            var result =  NotFound($"Chat session with id {id} has no chatbot specified");
            var responseDetails = new
            {
                StatusCode = result.StatusCode,
                Message = result.Value,
                Type = result.GetType().Name,
            };

            var fullResponse = JsonConvert.SerializeObject(responseDetails);
            yield return fullResponse;
            yield break;
        }
        
        if (chatSessionInfoDto.ChatbotModel is null or "")
        {
            var result =  BadRequest($"No model specified for chatbot {chatSessionInfoDto.ChatbotName}");
            var responseDetails = new
            {
                StatusCode = result.StatusCode,
                Message = result.Value,
                Type = result.GetType().Name,
            };

            var fullResponse = JsonConvert.SerializeObject(responseDetails);
            yield return fullResponse;
            yield break;
        }
        
        if (chatSessionInfoDto.Messages is null || chatSessionInfoDto.ApiHost is null or "" ||
            chatSessionInfoDto.ApiKey is null or "")
        {
            var result =  BadRequest(
                $"messages, API host and the API key must be specified for the chat session with id {id} " +
                $"for generating the assistant message");
            
            var responseDetails = new
            {
                StatusCode = result.StatusCode,
                Message = result.Value,
                Type = result.GetType().Name,
            };

            var fullResponse = JsonConvert.SerializeObject(responseDetails);
            yield return fullResponse;
            yield break;
        }

        Chatbot? chatbot = _chatbotService.GetChatbotByName(chatSessionInfoDto.ChatbotName);
        
        if (chatbot is null)
        {
            var result = NotFound($"Chat session with id {id} has no chatbot specified");
            var responseDetails = new
            {
                StatusCode = result.StatusCode,
                Message = result.Value,
                Type = result.GetType().Name,
            };

            var fullResponse = JsonConvert.SerializeObject(responseDetails);
            yield return fullResponse;
            yield break;
        }
        
        var chatbotModel = chatbot.TextGenerationChatbotModels
            .Find(tgm => tgm.Name == chatSessionInfoDto.ChatbotModel);

        if (chatbotModel is null)
        {
            var result =  NotFound(
                $"No model with name {chatSessionInfoDto.ChatbotModel} found for chatbot {chatSessionInfoDto.ChatbotName}");
            var responseDetails = new
            {
                StatusCode = result.StatusCode,
                Message = result.Value,
                Type = result.GetType().Name,
            };

            var fullResponse = JsonConvert.SerializeObject(responseDetails);
            yield return fullResponse;
            yield break;
        }

        var textGenerationRequestData = new TextGenerationRequestData
        {
            Messages = chatSessionInfoDto.Messages,
            ChatbotModel = chatbotModel,
            ApiHost = chatSessionInfoDto.ApiHost,
            ApiKey = chatSessionInfoDto.ApiKey,
            SystemInstruction = chatSessionInfoDto.SystemInstruction,
            Temperature = chatSessionInfoDto.Temperature
        };
        
        var output = new StringBuilder();
        
        await foreach (var chunk in _chatbotService.GenerateTextStreamInChatSession(chatbot, textGenerationRequestData))
        {
            output.Append(chunk);
            yield return chunk;
        }

        MessageDto assistantMessageDto = _messageService.CreateNewAssistantTextMessage(output.ToString(), 
            chatSessionInfoDto.ChatbotName, chatSessionInfoDto.ChatbotModel, "ok");
        await _messageService.AddNewMessageToChatSession(assistantMessageDto, chatSessionDto);
    }

    [HttpPost("new-assistant-image-message")]
    public async Task<ActionResult<MessageDto>> GenerateNewAssistantImageMessageFromChatSession(string id, 
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
        
        var chatbotModel = chatbot.ImageGenerationChatbotModels
            .Find(tgm => tgm.Name == chatSessionInfoDto.ChatbotModel);

        if (chatbotModel is null)
        {
            return NotFound(
                $"No model with name {chatSessionInfoDto.ChatbotModel} found for chatbot {chatSessionInfoDto.ChatbotName}");
        }

        var imageGenerationRequestData = new ImageGenerationRequestData()
        {
            LastUserMessage = chatSessionInfoDto.LastUserMessage,
            ChatbotModel = chatbotModel,
            ApiHost = chatSessionInfoDto.ApiHost,
            ApiKey = chatSessionInfoDto.ApiKey,
            ImageSize = chatSessionInfoDto.ImageSize,
            N = chatSessionInfoDto.ImagesToGenerate
        };

        var assistantMessageInfo = await _chatbotService.GenerateImageInChatSession(chatbot, imageGenerationRequestData);
        
        MessageDto assistantMessageDto = _messageService.CreateNewAssistantImageMessage(assistantMessageInfo.Text, 
            assistantMessageInfo.MediaDtos, chatSessionInfoDto.ChatbotName, chatSessionInfoDto.ChatbotModel, 
            assistantMessageInfo.Status);
        try
        {
            await _messageService.AddNewMessageToChatSession(assistantMessageDto, chatSessionDto);
        }
        catch (Exception e)
        {
            return StatusCode(500, $"Request Error: {e.Message}");
        }

        return Ok(assistantMessageDto);
    }
}