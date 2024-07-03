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
        _messageService.AddNewMessageToChatSession(userMessageDto, chatSessionDto);

        return Ok(userMessageDto);
    }
    
    [HttpPost("new-assistant-message")]
    public async Task<ActionResult<MessageDto>> GenerateNewAssistantMessageToChatSession(string id)
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

        var messageDtos = await _messageService.GetMessagesFromChatSession(chatSessionDto);
        
        string? content = await _chatbotService.GenerateAssistantMessageContentFromChatSession(chatbot, chatSessionDto,
            messageDtos);
        
        if (content is null)
        {
            return Problem("Problem occured while generating the assistant message");
        }

        MessageDto assistantMessageDto = _messageService.CreateNewAssistantMessage(content);
        _messageService.AddNewMessageToChatSession(assistantMessageDto, chatSessionDto);

        return Ok(assistantMessageDto);
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
        _messageService.AddNewMessageToChatSession(systemMessageDto, chatSessionDto);

        return Ok(systemMessageDto);
    }
}