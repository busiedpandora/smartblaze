namespace SmartBlaze.Backend.Models;

public class Chat
{
    private long _id;
    private string _title;
    private DateTime _creationDate;
    private List<Message>? _messages;

    
    public Chat(long id, string title)
    {
        this._id = id;
        this._title = title;
        this._creationDate = DateTime.Now;
    }
    
    public long Id
    {
        get => _id;
    }
    
    public string? Title
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