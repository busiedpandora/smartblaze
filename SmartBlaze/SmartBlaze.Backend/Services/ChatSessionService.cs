using System.Text;
using System.Text.Json;
using SmartBlaze.Backend.Dtos;
using SmartBlaze.Backend.Models;

namespace SmartBlaze.Backend.Services;

public class ChatSessionService
{
    private static long _counter = 0L;
    
    private List<ChatSession> _chatSessions;

    private HttpClient _httpClient;
    
    
    public ChatSessionService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        
        _chatSessions = new List<ChatSession>();
    }
    
    public List<ChatSession>? GetAllChatSessions()
    {
        return _chatSessions;
    }

    public ChatSession? GetChatSessionById(long id)
    {
        return _chatSessions.Find(chat => chat.Id.Equals(id));
    }
    
    public void AddNewMessageToChatSession(Message message, ChatSession chatSession)
    {
        chatSession.Messages.Add(message);
    }

    public ChatSession CreateNewChatSession(string title, Chatbot chatbot)
    {
        string model = chatbot.Models.ElementAt(0);
        return new ChatSession(++_counter, title, DateTime.Now, chatbot, model);
    }

    public void AddChatSession(ChatSession chatSession)
    {
        _chatSessions.Add(chatSession);
    }
}