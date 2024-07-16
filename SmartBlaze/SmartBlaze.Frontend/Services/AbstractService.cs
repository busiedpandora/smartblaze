namespace SmartBlaze.Frontend.Services;

public abstract class AbstractService(IHttpClientFactory httpClientFactory)
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("_httpClient");

    protected HttpClient HttpClient => _httpClient;
    
    public event Action? RefreshView;
    public event Action<string, string>? NavigateToErrorPage;
    public event Action<string>? NavigateToPage;
    
    protected void NotifyRefreshView() => RefreshView?.Invoke();
    protected void NotifyNavigateToErrorPage(string errorTitle, string errorMessage) 
        => NavigateToErrorPage?.Invoke(errorTitle, errorMessage);
    protected void NotifyNavigateToPage(string url) => NavigateToPage?.Invoke(url);
}