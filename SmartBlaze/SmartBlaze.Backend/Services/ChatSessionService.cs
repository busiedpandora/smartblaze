using SmartBlaze.Backend.Dtos;
using SmartBlaze.Backend.Models;

namespace SmartBlaze.Backend.Services;

public class ChatSessionService
{
    private static long _counter = 0L;
    
    private List<ChatSessionDto> _chatSessionDtos;

    private HttpClient _httpClient;
    
    
    public ChatSessionService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        
        _chatSessionDtos = new List<ChatSessionDto>();
    }
    
    public List<ChatSessionDto>? GetAllChatSessions()
    {
        return _chatSessionDtos;
    }

    public ChatSessionDto? GetChatSessionById(long id)
    {
        return _chatSessionDtos.Find(chat => chat.Id.Equals(id));
    }
    
    public void AddNewMessageToChatSession(MessageDto messageDto, ChatSessionDto chatSessionDto)
    {
        if (chatSessionDto.Messages is null)
        {
            chatSessionDto.Messages = new List<MessageDto>();
        }
        
        chatSessionDto.Messages.Add(messageDto);
    }

    public ChatSessionDto CreateNewChatSession(string title, Chatbot chatbot)
    {
        string model = chatbot.Models.ElementAt(0);
        
        return new ChatSessionDto()
        {
            Id = ++_counter,
            Title = title,
            CreationDate = DateTime.Now,
            ChatbotName = chatbot.Name,
            ChatbotModel = model
        };
    }

    public void AddChatSession(ChatSessionDto chatSessionDto)
    {
        _chatSessionDtos.Add(chatSessionDto);
    }
}