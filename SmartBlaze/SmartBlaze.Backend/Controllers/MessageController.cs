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
        if (messageDto is null || messageDto.Content is null)
        {
            return BadRequest("Message not specified correctly");
        }

        ChatSessionDto? chatSessionDto = await _chatSessionService.GetChatSessionById(id);
        if (chatSessionDto is null)
        {
            return NotFound($"Chat session with id {id} not found");
        }

        MessageDto userMessageDto = _messageService.CreateNewUserMessage(messageDto.Content);
        await _messageService.AddNewMessageToChatSession(userMessageDto, chatSessionDto);

        return Ok(userMessageDto);
    }
    
    [HttpPost("new-assistant-message")]
    public async Task<ActionResult<MessageDto>> GenerateNewAssistantMessageInChatSession(string id, 
        [FromBody] List<MessageDto> messageDtos)
    {
        ChatSessionDto? chatSessionDto = await _chatSessionService.GetChatSessionById(id);
        if (chatSessionDto is null)
        {
            return NotFound($"Chat session with id {id} not found");
        }

        if (chatSessionDto.ChatbotName is null)
        {
            return NotFound($"Chat session with id {id} has no chatbot specified");
        }

        Chatbot? chatbot = _chatbotService.GetChatbotByName(chatSessionDto.ChatbotName);
        
        if (chatbot is null)
        {
            return NotFound($"Chat session with id {id} has no chatbot specified");
        }
        
        string? content = await _chatbotService.GenerateTextInChatSession(chatbot, chatSessionDto,
            messageDtos);
        
        if (content is null)
        {
            return Problem("Problem occured while generating the assistant message");
        }

        MessageDto assistantMessageDto = _messageService.CreateNewAssistantMessage(content);
        await _messageService.AddNewMessageToChatSession(assistantMessageDto, chatSessionDto);

        return Ok(assistantMessageDto);
    }
    
    [HttpPost("new-assistant-stream-message")]
    public async IAsyncEnumerable<string> GenerateNewAssistantMessageInChatSessionStreamEnabled(string id, 
        [FromBody] List<MessageDto> messageDtos)
    {
        ChatSessionDto? chatSessionDto = await _chatSessionService.GetChatSessionById(id);
        if (chatSessionDto is null)
        {
            yield break;
        }

        if (chatSessionDto.ChatbotName is null)
        {
            yield break;
        }

        Chatbot? chatbot = _chatbotService.GetChatbotByName(chatSessionDto.ChatbotName);
        
        if (chatbot is null)
        {
            yield break;
        }
        
        var output = new StringBuilder();
        
        await foreach (var chunk in _chatbotService.GenerateTextStreamInChatSession(chatbot, chatSessionDto, messageDtos))
        {
            output.Append(chunk);
            yield return chunk;
        }

        MessageDto assistantMessageDto = _messageService.CreateNewAssistantMessage(output.ToString());
        await _messageService.AddNewMessageToChatSession(assistantMessageDto, chatSessionDto);
    }

    [HttpPost("new-system-message")]
    public async Task<ActionResult<MessageDto>> AddNewSystemMessageToChatSession(string id, [FromBody] MessageDto messageDto)
    {
        if (messageDto is null || messageDto.Content is null)
        {
            return BadRequest("Message not specified correctly");
        }
        
        ChatSessionDto? chatSessionDto = await _chatSessionService.GetChatSessionById(id);
        if (chatSessionDto is null)
        {
            return NotFound($"Chat session with id {id} not found");
        }

        MessageDto systemMessageDto = _messageService.CreateNewSystemMessage(messageDto.Content);
        await _messageService.AddNewMessageToChatSession(systemMessageDto, chatSessionDto);

        return Ok(systemMessageDto);
    }
}