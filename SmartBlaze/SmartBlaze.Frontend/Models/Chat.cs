namespace SmartBlaze.Frontend.Models;

public class Chat
{
    private long _id;
    private string _title;
    private bool _selected;


    public Chat(long id, string title)
    {
        this._id = id;
        this._title = title;
        this._selected = false;
    }

    public long Id
    {
        get => _id;
    }

    public string Title
    {
        get => _title;
    }

    public bool Selected
    {
        get => _selected;
        set => _selected = value;
    }
}