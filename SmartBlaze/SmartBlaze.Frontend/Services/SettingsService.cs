using SmartBlaze.Frontend.Dtos;
using SmartBlaze.Frontend.Models;

namespace SmartBlaze.Frontend.Services;

public class SettingsService(IHttpClientFactory httpClientFactory) : AbstractService(httpClientFactory)
{
    private List<Chatbot>? _chatbots;
    private Chatbot? _chatbotSelected;
    //private string _chatbotSelectedModel = "";

    private string _systemInstruction = "";
    private bool _textStream;
    
    private bool _settingsPageOpen = false;
    private string _settingsMenuSelected = "model";
    
    
    public bool SettingsPageOpen
    {
        get => _settingsPageOpen;
    }

    public string SettingsMenuSelected => _settingsMenuSelected;
    
    public List<Chatbot>? Chatbots => _chatbots;

    public Chatbot? ChatbotSelected => _chatbotSelected;
    
    public string SystemInstruction => _systemInstruction;

    public bool TextStream => _textStream;

    public Chatbot? GetChatbotByName(string chatbotName)
    {
        var chatbot = _chatbots?.Find(c => c.Name == chatbotName);

        return chatbot;
    }

    public async Task SetUpConfiguration()
    {
        await SetUpChatbotConfiguration();
        await LoadChatbots();
        //await LoadDefaultChatSessionConfigurations();
    }
    
    public void OpenChatbotSettings()
    {
        _settingsPageOpen = true;
        _settingsMenuSelected = "chatbot";
        
        NotifyNavigateToPage("/settings/chatbot");
        NotifyRefreshView();
    }
    
    public void OpenChatSessionSettings()
    {
        _settingsPageOpen = true;
        _settingsMenuSelected = "chat";
        
        NotifyNavigateToPage("/settings/chat");
        NotifyRefreshView();
    }

    public void CloseSettings()
    {
        _settingsPageOpen = false;
        
        NotifyRefreshView();
    }

    public async Task SaveChatbotSettings(ChatbotSettings chatbotSettings)
    {
        var chatbot = _chatbots?.Find(c => c.Name == chatbotSettings.ChatbotName);

        if (chatbot is not null)
        {
            chatbot.Model = chatbotSettings.ChatbotModel;
            chatbot.Apihost = chatbotSettings.ApiHost;
            chatbot.ApiKey = chatbotSettings.ApiKey;
            
            SelectChatbot(chatbot);

            var chatbotDto = new ChatbotDto()
            {
                Name = chatbot.Name,
                Model = chatbot.Model,
                ApiHost = chatbot.Apihost,
                ApiKey = chatbot.ApiKey,
                Selected = true,
            };

            var chatbotConfigurationResponse = await HttpClient.PostAsJsonAsync("configuration/chatbot", chatbotDto);
            var chatbotConfigurationResponseContent = await chatbotConfigurationResponse.Content.ReadAsStringAsync();

            if (!chatbotConfigurationResponse.IsSuccessStatusCode)
            {
                NotifyNavigateToErrorPage($"Error occured while configuring the chatbot {chatbot.Name}",
                    chatbotConfigurationResponseContent);
            }
        }
    }

    public void SaveChatSessionSettings(ChatSessionSettings chatSessionSettings)
    {
        _systemInstruction = chatSessionSettings.SystemInstruction;
        _textStream = chatSessionSettings.TextStream;
    }
    
    private void SelectChatbot(Chatbot chatbot)
    {
        if (_chatbots is null)
        {
            return;
        }

        if (chatbot.Models.Count == 0)
        {
            NotifyNavigateToErrorPage($"Error occured while selecting the chatbot {chatbot.Name}", 
                $"No models for chatbot {chatbot.Name} found");
            NotifyRefreshView();
            return;
        }

        _chatbotSelected = chatbot;
    }

    private async Task SetUpChatbotConfiguration()
    {
        await HttpClient.PostAsync("configuration/chatbot/default", null);
    }
    
    private async Task LoadChatbots()
    {
        if (_chatbots is not null)
        {
            return;
        }

        var chatbotDtos = await HttpClient.GetFromJsonAsync<List<ChatbotDto>>("configuration/chatbot");

        if (chatbotDtos is null || chatbotDtos.Count == 0)
        {
            NotifyNavigateToErrorPage("Error occured while loading the chatbots", "No chatbot found");
            return;
        }

        var chatbots = new List<Chatbot>();
        Chatbot? chatbotToSelect = null;

        foreach (var chatbotDto in chatbotDtos)
        {
            if (chatbotDto.Name is not null && chatbotDto.Models is not null && chatbotDto.ApiHost is not null 
                && chatbotDto.ApiKey is not null && chatbotDto.Model is not null)
            {
                var chatbot = new Chatbot(chatbotDto.Name, chatbotDto.Models, chatbotDto.ApiHost, chatbotDto.ApiKey, chatbotDto.Model);
                chatbots.Add(chatbot);

                if (chatbotDto.Selected is not null && chatbotDto.Selected == true)
                {
                    chatbotToSelect = chatbot;
                }
            }
        }

        _chatbots = chatbots;
        
        if (_chatbots is null || _chatbots.Count == 0)
        {
            NotifyNavigateToErrorPage("Error occured while loading the chatbots", "No chatbot found");
            return;
        }
        
        if (chatbotToSelect?.Models.Count == 0)
        {
            NotifyNavigateToErrorPage("Error occured while loading the chatbots", 
                $"No model found for chatbot {chatbotToSelect.Name}");
            return;
        }

        if (chatbotToSelect is null)
        {
            chatbotToSelect = _chatbots.ElementAt(0);
        }
        
        SelectChatbot(chatbotToSelect);
    }

    private async Task LoadDefaultChatSessionConfigurations()
    {
        var chatSessionDefaultConfig = await HttpClient.GetFromJsonAsync<ChatSessionSettingsDto>
            ("configuration/default/chat-session");

        if (chatSessionDefaultConfig is null || chatSessionDefaultConfig.SystemInstruction is null 
            || chatSessionDefaultConfig.TextStream is null)
        {
            NotifyNavigateToErrorPage("Error occured while setting up the configurations", 
                "The default configuration for the chat session have not been loaded correctly");
            return;
        }
        
        _systemInstruction = chatSessionDefaultConfig.SystemInstruction;
        _textStream = chatSessionDefaultConfig.TextStream ?? false;
    }
}