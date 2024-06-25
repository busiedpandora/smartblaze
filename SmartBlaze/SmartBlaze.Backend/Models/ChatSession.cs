namespace SmartBlaze.Backend.Models;

public class ChatSession
{
    private long _id;
    private string _title;
    private DateTime _creationDate;
    private List<Message> _messages;

    
    public ChatSession(long id, string title, DateTime creationDate)
    {
        this._id = id;
        this._title = title;
        this._creationDate = creationDate;
        this._messages = new List<Message>();
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
}