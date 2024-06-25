namespace SmartBlaze.Frontend.Models;

public class ChatSession
{
    private long _id;
    private string _title;
    private DateTime _creationDate;
    private List<Message> _messages;
    private bool _selected;

    
    public ChatSession(long id, string title, DateTime creationDate)
    {
        _id = id;
        _title = title;
        _creationDate = creationDate;
    }
    
    public long Id
    {
        get => _id;
        set => _id = value;
    }

    public string? Title
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

    public bool Selected
    {
        get => _selected;
        set => _selected = value;
    }
}