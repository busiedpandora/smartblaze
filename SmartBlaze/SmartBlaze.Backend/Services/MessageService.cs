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

    public MessageDto CreateNewUserMessage(string text, List<MediaDto>? mediaDtos)
    {
        return new MessageDto()
        {
            Text = text,
            Role = Role.User,
            CreationDate = DateTime.Now,
            MediaDtos = mediaDtos
        };
    }
    
    public MessageDto CreateNewAssistantMessage(string text)
    {
        return new MessageDto()
        {
            Text = text,
            Role = Role.Assistant,
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