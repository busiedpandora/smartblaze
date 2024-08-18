namespace SmartBlaze.Frontend.Services;

public class RedirectionService
{
    private string _url = "/welcome";
    private string _errorTitle = string.Empty;
    private string _errorMessage = string.Empty;

    public string Url
    {
        get => _url;
        set => _url = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string ErrorTitle
    {
        get => _errorTitle;
        set => _errorTitle = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => _errorMessage = value ?? throw new ArgumentNullException(nameof(value));
    }
}