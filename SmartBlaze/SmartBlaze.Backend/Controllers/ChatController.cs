using SmartBlaze.Backend.Models;

namespace SmartBlaze.Backend.Controllers;

public class ChatController
{
    private List<Chat> chats;

    
    public ChatController()
    {
        //chats = new List<Chat>();
        chats = new List<Chat>() {new Chat(1L, "chat1"), new Chat(2L, "chat2")};
    }

    public List<Chat> GetAllChats()
    {
        return chats;
    }

    public Chat? GetChatById(long id)
    {
        return chats.Find(chat => chat.Id.Equals(id));
    }
    
    
}