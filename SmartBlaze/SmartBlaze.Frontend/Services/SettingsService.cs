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
        await LoadChatbotDefaultConfiguration();
        await LoadChatSessionDefaultConfiguration();
    }
    
    public void OpenChatbotSettings()
    {
        _settingsPageOpen = true;
        _settingsMenuSelected = "chatbot";
        
        NotifyNavigateToPage("/settings/chatbot");
        NotifyRefreshView();
    }
    
    public void OpenChatSessionDefaultSettings()
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

    public async Task SaveChatbotDefaultSettings(ChatbotSettings chatbotSettings)
    {
        var chatbot = _chatbots?.Find(c => c.Name == chatbotSettings.ChatbotName);

        if (chatbot is not null)
        {
            chatbot.Model = chatbotSettings.ChatbotModel;
            chatbot.ApiHost = chatbotSettings.ApiHost;
            chatbot.ApiKey = chatbotSettings.ApiKey;
            chatbot.Temperature = (float)Math.Round(chatbotSettings.Temperature, 1);
            
            SelectChatbot(chatbot);

            var chatbotConfigurationDto = new ChatbotDefaultConfigurationDto()
            {
                ChatbotName = chatbot.Name,
                ChatbotModel = chatbot.Model,
                ApiHost = chatbot.ApiHost,
                ApiKey = chatbot.ApiKey,
                TextStreamDelay = chatbot.TextStreamDelay,
                Temperature = chatbot.Temperature,
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

    public async Task SaveChatSessionDefaultSettings(ChatSessionDefaultSettings chatSessionDefaultSettings)
    {
        var chatSessionDefaultConfiguration = new ChatSessionDefaultConfigurationDto()
        {
            SystemInstruction = chatSessionDefaultSettings.SystemInstruction,
            TextStream = chatSessionDefaultSettings.TextStream
        };
        
        var chatSessionDefaultConfigurationResponse = await HttpClient.PostAsJsonAsync("configuration/chat-session", 
            chatSessionDefaultConfiguration);

        if (!chatSessionDefaultConfigurationResponse.IsSuccessStatusCode)
        {
            var chatSessionDefaultConfigurationResponseContent =
                await chatSessionDefaultConfigurationResponse.Content.ReadAsStringAsync();
            NotifyNavigateToErrorPage("Error occured while configuring the chat session", 
                chatSessionDefaultConfigurationResponseContent);
            return;
        }
        
        _systemInstruction = chatSessionDefaultSettings.SystemInstruction;
        _textStream = chatSessionDefaultSettings.TextStream;
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
    
    private async Task LoadChatbotDefaultConfiguration()
    {
        if (_chatbots is not null)
        {
            return;
        }

        var chatbotConfigurationDtos = await HttpClient.GetFromJsonAsync<List<ChatbotDefaultConfigurationDto>>("configuration/chatbot");

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
            if (chatbotConfigurationDto.ChatbotName is not null && chatbotConfigurationDto.ChatbotModels is not null 
                && chatbotConfigurationDto.ApiHost is not null && chatbotConfigurationDto.ApiKey is not null 
                && chatbotConfigurationDto.ChatbotModel is not null)
            {
                var chatbot = new Chatbot(chatbotConfigurationDto.ChatbotName, chatbotConfigurationDto.ChatbotModels,
                    chatbotConfigurationDto.MinTemperature, chatbotConfigurationDto.MaxTemperature);
                chatbot.ApiHost = chatbotConfigurationDto.ApiHost;
                chatbot.ApiKey = chatbotConfigurationDto.ApiKey;
                chatbot.Model = chatbotConfigurationDto.ChatbotModel;
                chatbot.TextStreamDelay = chatbotConfigurationDto.TextStreamDelay;
                chatbot.Temperature = chatbotConfigurationDto.Temperature;
                
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

    private async Task LoadChatSessionDefaultConfiguration()
    {
        var chatSessionDefaultConfigurationDto = await HttpClient.GetFromJsonAsync<ChatSessionDefaultConfigurationDto>("configuration/chat-session");

        if (chatSessionDefaultConfigurationDto is null)
        {
            NotifyNavigateToErrorPage("Error occured while loading the chat session configuration", 
                "No configuration found");
            return;
        }

        if (chatSessionDefaultConfigurationDto.SystemInstruction is null)
        {
            NotifyNavigateToErrorPage("Error occured while loading the chat session configuration", 
                "The system instruction and text stream must be specified");
            return;
        }
        
        _systemInstruction = chatSessionDefaultConfigurationDto.SystemInstruction;
        _textStream = chatSessionDefaultConfigurationDto.TextStream;
    }
}