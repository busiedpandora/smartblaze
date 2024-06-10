namespace SmartBlaze.Backend.Models;

public class Chatbot
{
    private string _name;
    private List<string> _models;
    private string _apihost;
    private string _apikey;

    
    public Chatbot(string name, List<string> models, string apihost, string apikey)
    {
        _name = name;
        _models = models;
        _apihost = apihost;
        _apikey = apikey;
    }

    public string Name
    {
        get => _name;
    }

    public List<string> Models
    {
        get => _models;
    }

    public string Apihost
    {
        get => _apihost;
    }

    public string Apikey
    {
        get => _apikey;
    }
}