using SmartBlaze.Backend.Dtos;

namespace SmartBlaze.Backend.Services;

public interface IMessageService
{
    MessageDto CreateNewUserMessage(string text, List<MediaDto>? mediaDtos);
    MessageDto CreateNewAssistantTextMessage(string text, string chatbotName, string chatbotModel, string status);

    MessageDto CreateNewAssistantImageMessage(string? text, List<MediaDto>? mediaDtos, string chatbotName,
        string chatbotModel, string status);

    Task AddNewMessageToChatSession(MessageDto messageDto, ChatSessionDto chatSessionDto);
    Task<List<MessageDto>> GetMessagesFromChatSession(ChatSessionDto chatSessionDto);
}