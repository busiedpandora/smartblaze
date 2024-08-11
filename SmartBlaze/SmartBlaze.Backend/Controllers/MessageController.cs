using System.Text;
using Microsoft.AspNetCore.Mvc;
using SmartBlaze.Backend.Dtos;
using SmartBlaze.Backend.Models;
using SmartBlaze.Backend.Services;

namespace SmartBlaze.Backend.Controllers;

[ApiController]
[Route("chat-session/{id}")]
public class MessageController : ControllerBase
{
    private ChatSessionService _chatSessionService;
    private MessageService _messageService;
    private ChatbotService _chatbotService;


    public MessageController(ChatSessionService chatSessionService, MessageService messageService, 
        ChatbotService chatbotService)
    {
        _chatSessionService = chatSessionService;
        _messageService = messageService;
        _chatbotService = chatbotService;
    }
    
    [HttpGet("messages")]
    public async Task<ActionResult<List<MessageDto>>> GetMessagesFromChatSession(string id)
    {
        ChatSessionDto? chatSessionDto = await _chatSessionService.GetChatSessionById(id);
        
        if (chatSessionDto is null)
        {
            return NotFound($"Chat session with id {id} not found");
        }

        var messages = await _messageService.GetMessagesFromChatSession(chatSessionDto);

        return messages;
    }
    
    [HttpPost("new-user-message")]
    public async Task<ActionResult<MessageDto>> AddNewUserMessageToChatSession(string id, [FromBody] MessageDto messageDto)
    {
        if (messageDto.Text is null)
        {
            return BadRequest("Message not specified correctly");
        }

        ChatSessionDto? chatSessionDto = await _chatSessionService.GetChatSessionById(id);
        if (chatSessionDto is null)
        {
            return NotFound($"Chat session with id {id} not found");
        }

        MessageDto userMessageDto = _messageService.CreateNewUserMessage(messageDto.Text, messageDto.MediaDtos);
        await _messageService.AddNewMessageToChatSession(userMessageDto, chatSessionDto);

        return Ok(userMessageDto);
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

        if (chatSessionInfoDto.ChatbotName is null)
        {
            return NotFound($"Chat session with id {id} has no chatbot specified");
        }

        if (chatSessionInfoDto.ChatbotModel is null)
        {
            return BadRequest($"No model specified for chatbot {chatSessionInfoDto.ChatbotName}");
        }

        if (chatSessionInfoDto.Messages is null || chatSessionInfoDto.ApiHost is null ||
            chatSessionInfoDto.ApiKey is null)
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
        
        string? content = await _chatbotService.GenerateTextInChatSession(chatbot, chatSessionInfoDto);
        
        if (content is null)
        {
            return Problem("Problem occured while generating the assistant message");
        }

        MessageDto assistantMessageDto = _messageService.CreateNewAssistantTextMessage(content, 
            chatSessionInfoDto.ChatbotName, chatSessionInfoDto.ChatbotModel);
        await _messageService.AddNewMessageToChatSession(assistantMessageDto, chatSessionDto);

        return Ok(assistantMessageDto);
    }
    
    [HttpPost("new-assistant-empty-message")]
    public ActionResult<MessageDto> GetNewAssistantMessageWithEmptyContent(string id, [FromBody] ChatSessionInfoDto chatSessionInfoDto)
    {
        if (chatSessionInfoDto.ChatbotName is null)
        {
            return NotFound($"Chat session with id {id} has no chatbot specified");
        }

        if (chatSessionInfoDto.ChatbotModel is null)
        {
            return BadRequest($"No model specified for chatbot {chatSessionInfoDto.ChatbotName}");
        }
        
        MessageDto assistantMessageDto = _messageService.CreateNewAssistantTextMessage("", 
            chatSessionInfoDto.ChatbotName, chatSessionInfoDto.ChatbotModel);

        return Ok(assistantMessageDto);
    }
    
    [HttpPost("generate-assistant-stream-message")]
    public async IAsyncEnumerable<string> GenerateNewAssistantTextMessageInChatSessionStreamEnabled(string id, 
        [FromBody] ChatSessionInfoDto chatSessionInfoDto)
    {
        ChatSessionDto? chatSessionDto = await _chatSessionService.GetChatSessionById(id);
        if (chatSessionDto is null)
        {
            yield break;
        }

        if (chatSessionInfoDto.ChatbotName is null)
        {
            yield break;
        }
        
        if (chatSessionInfoDto.ChatbotModel is null)
        {
            yield break;
        }
        
        if (chatSessionInfoDto.Messages is null || chatSessionInfoDto.ApiHost is null ||
            chatSessionInfoDto.ApiKey is null)
        {
            yield break;
        }

        Chatbot? chatbot = _chatbotService.GetChatbotByName(chatSessionInfoDto.ChatbotName);
        
        if (chatbot is null)
        {
            yield break;
        }
        
        var output = new StringBuilder();
        
        await foreach (var chunk in _chatbotService.GenerateTextStreamInChatSession(chatbot, chatSessionInfoDto))
        {
            output.Append(chunk);
            yield return chunk;
        }

        MessageDto assistantMessageDto = _messageService.CreateNewAssistantTextMessage(output.ToString(), 
            chatSessionInfoDto.ChatbotName, chatSessionInfoDto.ChatbotModel);
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

        if (chatSessionInfoDto.ChatbotName is null)
        {
            return NotFound($"Chat session with id {id} has no chatbot specified");
        }

        if (chatSessionInfoDto.ChatbotModel is null)
        {
            return BadRequest($"No model specified for chatbot {chatSessionInfoDto.ChatbotName}");
        }
        
        if (chatSessionInfoDto.LastUserMessage is null || chatSessionInfoDto.ApiHost is null ||
            chatSessionInfoDto.ApiKey is null)
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

        var assistantMessageInfo = await _chatbotService.GenerateImageInChatSession(chatbot, chatSessionInfoDto);
        
        MessageDto assistantMessageDto = _messageService.CreateNewAssistantImageMessage(assistantMessageInfo.Text, 
            assistantMessageInfo.MediaDtos, chatSessionInfoDto.ChatbotName, chatSessionInfoDto.ChatbotModel, 
            assistantMessageInfo.Status);
        
        await _messageService.AddNewMessageToChatSession(assistantMessageDto, chatSessionDto);

        return Ok(assistantMessageDto);
    }
}