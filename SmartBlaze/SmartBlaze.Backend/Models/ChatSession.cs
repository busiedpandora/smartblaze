namespace SmartBlaze.Backend.Models;

public class ChatSession
{
    private long _id;
    private string _title;
    private DateTime _creationDate;
    private List<Message> _messages;

    
    public ChatSession(long id, string title)
    {
        this._id = id;
        this._title = title;
        this._creationDate = DateTime.Now;
        this._messages = new List<Message>();
    }
    
    public long Id
    {
        get => _id;
    }
    
    public string Title
    {
        get => _title;
    }
    
    public DateTime CreationDate
    {
        get => _creationDate;
    }
    
    public List<Message> Messages
    {
        get => _messages;
    }
}