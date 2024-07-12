namespace SmartBlaze.Frontend.Services;

public class SettingsService(IHttpClientFactory httpClientFactory) : AbstractService(httpClientFactory)
{
    private bool _settingsPageOpen = false;


    public bool SettingsPageOpen
    {
        get => _settingsPageOpen;
    }

    public void OpenModelsSettings()
    {
        _settingsPageOpen = true;
        
        NotifyNavigateToPage("/settings/models");
        NotifyRefreshView();
    }

    public void CloseSettings()
    {
        _settingsPageOpen = false;
        
        NotifyRefreshView();
    }
}