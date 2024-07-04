using SmartBlaze.Backend.Dtos;
using SmartBlaze.Backend.Models;
using SmartBlaze.Backend.Repositories;

namespace SmartBlaze.Backend.Services;

public class MessageService
{
    private MessageRepository _messageRepository;


    public MessageService(MessageRepository messageRepository)
    {
        _messageRepository = messageRepository;
    }

    public MessageDto CreateNewUserMessage(string content)
    {
        return new MessageDto()
        {
            Content = content,
            Role = Role.User,
            CreationDate = DateTime.Now
        };
    }
    
    public MessageDto CreateNewAssistantMessage(string content)
    {
        return new MessageDto()
        {
            Content = content,
            Role = Role.Assistant,
            CreationDate = DateTime.Now
        };
    }
    
    public MessageDto CreateNewSystemMessage(string content)
    {
        return new MessageDto()
        {
            Content = content,
            Role = Role.System,
            CreationDate = DateTime.Now
        };
    }
    
    public async Task AddNewMessageToChatSession(MessageDto messageDto, ChatSessionDto chatSessionDto)
    {
        await _messageRepository.SaveMessage(messageDto, chatSessionDto.Id);
    }

    public async Task<List<MessageDto>> GetMessagesFromChatSession(ChatSessionDto chatSessionDto)
    {
        var messages = await _messageRepository.GetMessagesFromChatSession(chatSessionDto.Id);

        return messages;
    }
}