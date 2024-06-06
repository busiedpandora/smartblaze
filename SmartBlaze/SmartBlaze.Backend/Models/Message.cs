namespace SmartBlaze.Backend.Models;

public class Message
{
    private string _content;
    private string _role;

    public Message(string content, string role)
    {
        _content = content;
        _role = role;
    }

    public string Content
    {
        get => _content;
    }

    public string Role
    {
        get => _role;
    }
}