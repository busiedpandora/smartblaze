using SmartBlaze.Backend.Models;

namespace SmartBlaze.Backend.Services;

public class ChatSessionService
{
    private List<ChatSession> _chats;


    public ChatSessionService()
    {
        //_chats = new List<ChatSession>();
        _chats = new List<ChatSession>() {new ChatSession(1L, "chat1"), new ChatSession(2L, "chat2")};
    }
    
    public List<ChatSession> GetAllChats()
    {
        return _chats;
    }

    public ChatSession? GetChatById(long id)
    {
        return _chats.Find(chat => chat.Id.Equals(id));
    }
}