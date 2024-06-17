using System.Text.Json.Serialization;

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
    
    [JsonPropertyName("content")]
    public string Content
    {
        get => _content;
    }

    [JsonPropertyName("role")]
    public string Role
    {
        get => _role;
    }

    public DateTime CreationDate
    {
        get => _creationDate;
    }
}