namespace SmartBlaze.Backend.Models;

public class Message
{
    private string _content;
    private string _role;
    private DateTime _creationDate;

    
    public Message(string content, string role, DateTime creationDate)
    {
        _content = content;
        _role = role;
        _creationDate = creationDate;
    }

    public string Content
    {
        get => _content;
        set => _content = value;
    }

    public string Role
    {
        get => _role;
        set => _role = value;
    }

    public DateTime CreationDate
    {
        get => _creationDate;
        set => _creationDate = value;
    }
}