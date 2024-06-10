namespace SmartBlaze.Backend.Dtos;

public class ChatSessionDto
{
    private long _id;
    private string _title;
    
    
    public ChatSessionDto(long id, string title)
    {
        this._id = id;
        this._title = title;
    }
    
    public long Id
    {
        get => _id;
    }
    
    public string? Title
    {
        get => _title;
    }
}