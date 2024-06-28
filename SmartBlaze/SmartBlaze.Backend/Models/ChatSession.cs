namespace SmartBlaze.Backend.Models;

public class ChatSession
{
    private long _id;
    private string _title;
    private DateTime _creationDate;
    private List<Message> _messages;
    private Chatbot _chatbot;
    private string _model;

    
    public ChatSession(long id, string title, DateTime creationDate, Chatbot chatbot, string model)
    {
        _id = id;
        _title = title;
        _creationDate = creationDate;
        _messages = new List<Message>();
        _chatbot = chatbot;
        _model = model;
    }
    
    public long Id
    {
        get => _id;
        set => _id = value;
    }
    
    public string Title
    {
        get => _title;
        set => _title = value;
    }
    
    public DateTime CreationDate
    {
        get => _creationDate;
        set => _creationDate = value;
    }
    
    public List<Message> Messages
    {
        get => _messages;
        set => _messages = value;
    }

    public Chatbot Chatbot
    {
        get => _chatbot;
        set => _chatbot = value;
    }

    public string Model
    {
        get => _model;
        set => _model = value;
    }
}