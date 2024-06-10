using SmartBlaze.Backend.Models;

namespace SmartBlaze.Backend.Services;

public class ChatSessionService
{
    private List<ChatSession> _chatSessions;


    public ChatSessionService()
    {
        //_chatSessions = new List<ChatSession>();
        
        ChatSession cs1 = new ChatSession(1L, "chat1");
        cs1.Messages.Add(new Message("Hello. I'm here to assist you!", "system"));
        cs1.Messages.Add(new Message("What is the capital of Germany?", "user"));
        cs1.Messages.Add(new Message("The capital of Germany is Berlin", "assistant"));
        
        ChatSession cs2 = new ChatSession(2L, "chat2");
        cs2.Messages.Add(new Message("Hello. I'm here to assist you!", "system"));
        cs2.Messages.Add(new Message("What time is it?", "user"));
        cs2.Messages.Add(new Message("It is 3.00 pm.", "assistant"));
        
        _chatSessions = new List<ChatSession>() {cs1, cs2};
    }
    
    public List<ChatSession> GetAllChatSessions()
    {
        return _chatSessions;
    }

    public ChatSession? GetChatSessionById(long id)
    {
        return _chatSessions.Find(chat => chat.Id.Equals(id));
    }
}