using System.Text.Json.Serialization;

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
    
    [JsonPropertyName("id")]
    public long Id
    {
        get => _id;
    }
    
    [JsonPropertyName("title")]
    public string Title
    {
        get => _title;
    }
    
    [JsonPropertyName("creationDate")]
    public DateTime CreationDate
    {
        get => _creationDate;
    }
    
    [JsonPropertyName("messages")]
    public List<Message> Messages
    {
        get => _messages;
    }
}