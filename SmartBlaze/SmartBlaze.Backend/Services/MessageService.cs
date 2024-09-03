using SmartBlaze.Backend.Dtos;
using SmartBlaze.Backend.Models;
using SmartBlaze.Backend.Repositories;

namespace SmartBlaze.Backend.Services;

public class MessageService : IMessageService
{
    private readonly IMessageRepository _messageRepository;


    public MessageService(IMessageRepository messageRepository)
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
    
    public MessageDto CreateNewAssistantTextMessage(string text, string chatbotName, string chatbotModel, string status)
    {
        return new MessageDto()
        {
            Text = text,
            Role = Role.Assistant,
            CreationDate = DateTime.Now,
            ChatbotName = chatbotName,
            ChatbotModel = chatbotModel,
            Status = status
        };
    }

    public MessageDto CreateNewAssistantImageMessage(string? text, List<MediaDto>? mediaDtos, string chatbotName, 
        string chatbotModel, string status)
    {
        return new MessageDto()
        {
            Text = text,
            MediaDtos = mediaDtos,
            Role = Role.Assistant,
            CreationDate = DateTime.Now,
            ChatbotName = chatbotName,
            ChatbotModel = chatbotModel,
            Status = status
        };
    }
    
    public async Task AddNewMessageToChatSession(MessageDto messageDto, ChatSessionDto chatSessionDto)
    {
        await _messageRepository.SaveMessage(messageDto, chatSessionDto.Id ?? "");
    }

    public async Task<List<MessageDto>> GetMessagesFromChatSession(ChatSessionDto chatSessionDto)
    {
        var messages = await _messageRepository.GetMessagesFromChatSession(chatSessionDto.Id ?? "");

        return messages;
    }
}