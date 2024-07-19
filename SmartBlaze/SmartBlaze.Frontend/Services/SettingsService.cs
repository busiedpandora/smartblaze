using SmartBlaze.Frontend.Dtos;
using SmartBlaze.Frontend.Models;

namespace SmartBlaze.Frontend.Services;

public class SettingsService(IHttpClientFactory httpClientFactory) : AbstractService(httpClientFactory)
{
    private List<Chatbot>? _chatbots;
    private Chatbot? _chatbotSelected;

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
        await LoadChatbotConfiguration();
        await LoadChatSessionConfiguration();
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

            var chatbotConfigurationDto = new ChatbotConfigurationDto()
            {
                ChatbotName = chatbot.Name,
                ChatbotModel = chatbot.Model,
                ApiHost = chatbot.Apihost,
                ApiKey = chatbot.ApiKey,
                TextStreamDelay = chatbot.TextStreamDelay,
                Selected = true,
            };

            var chatbotConfigurationResponse = await HttpClient.PostAsJsonAsync("configuration/chatbot", chatbotConfigurationDto);

            if (!chatbotConfigurationResponse.IsSuccessStatusCode)
            {
                var chatbotConfigurationResponseContent = await chatbotConfigurationResponse.Content.ReadAsStringAsync();
                NotifyNavigateToErrorPage($"Error occured while configuring the chatbot {chatbot.Name}",
                    chatbotConfigurationResponseContent);
            }
        }
    }

    public async Task SaveChatSessionSettings(ChatSessionSettings chatSessionSettings)
    {
        var chatSessionConfigurationResponse = await HttpClient.PostAsJsonAsync("configuration/chat-session", 
            chatSessionSettings);

        if (!chatSessionConfigurationResponse.IsSuccessStatusCode)
        {
            var chatSessionConfigurationResponseContent =
                await chatSessionConfigurationResponse.Content.ReadAsStringAsync();
            NotifyNavigateToErrorPage("Error occured while configuring the chat session", 
                chatSessionConfigurationResponseContent);
            return;
        }
        
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
    
    private async Task LoadChatbotConfiguration()
    {
        if (_chatbots is not null)
        {
            return;
        }

        var chatbotConfigurationDtos = await HttpClient.GetFromJsonAsync<List<ChatbotConfigurationDto>>("configuration/chatbot");

        if (chatbotConfigurationDtos is null || chatbotConfigurationDtos.Count == 0)
        {
            NotifyNavigateToErrorPage("Error occured while loading the chatbot configurations", 
                "No chatbot configuration found");
            return;
        }

        var chatbots = new List<Chatbot>();
        Chatbot? chatbotToSelect = null;

        foreach (var chatbotConfigurationDto in chatbotConfigurationDtos)
        {
            if (chatbotConfigurationDto.ChatbotName is not null && chatbotConfigurationDto.Models is not null 
                && chatbotConfigurationDto.ApiHost is not null && chatbotConfigurationDto.ApiKey is not null 
                && chatbotConfigurationDto.ChatbotModel is not null)
            {
                var chatbot = new Chatbot(chatbotConfigurationDto.ChatbotName, chatbotConfigurationDto.Models, 
                    chatbotConfigurationDto.ApiHost, chatbotConfigurationDto.ApiKey, 
                    chatbotConfigurationDto.ChatbotModel, chatbotConfigurationDto.TextStreamDelay);
                chatbots.Add(chatbot);

                if (chatbotConfigurationDto.Selected)
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

    private async Task LoadChatSessionConfiguration()
    {
        var chatSessionSettings = await HttpClient.GetFromJsonAsync<ChatSessionSettingsDto>("configuration/chat-session");

        if (chatSessionSettings is null)
        {
            NotifyNavigateToErrorPage("Error occured while loading the chat session configuration", 
                "No configuration found");
            return;
        }

        if (chatSessionSettings.SystemInstruction is null || chatSessionSettings.TextStream is null)
        {
            NotifyNavigateToErrorPage("Error occured while loading the chat session configuration", 
                "The system instruction and text stream must be specified");
            return;
        }
        
        _systemInstruction = chatSessionSettings.SystemInstruction;
        _textStream = chatSessionSettings.TextStream.Value;
    }
}