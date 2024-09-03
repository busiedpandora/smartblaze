using SmartBlaze.Frontend.Dtos;
using SmartBlaze.Frontend.Models;

namespace SmartBlaze.Frontend.Services;

public class SettingsService(IHttpClientFactory httpClientFactory) : AbstractService(httpClientFactory)
{
    private UserStateService _userStateService;
    
    private List<ChatbotDto>? _chatbots;
    
    private ChatbotDto? _chatbotSelectedInCurrentChatSession;
    private ChatbotModelDto? _chatbotModelSelectedInCurrentChatSession;

    private List<ChatbotDefaultConfigurationDto>? _chatbotDefaultConfigurations;
    
    private ChatbotDefaultConfigurationDto? _chatbotDefaultConfigurationSelected;
    private ChatSessionDefaultConfigurationDto? _chatSessionDefaultConfiguration;
    
    private bool _settingsPageOpen = false;
    private string _settingsMenuSelected = "chatbot";


    public SettingsService(IHttpClientFactory httpClientFactory, UserStateService userStateService) : this(httpClientFactory)
    {
        _userStateService = userStateService;
    }

    public List<ChatbotDto>? Chatbots => _chatbots;

    public ChatbotDto? ChatbotSelectedInCurrentChatSession
    {
        get => _chatbotSelectedInCurrentChatSession;
        set => _chatbotSelectedInCurrentChatSession = value;
    }

    public ChatbotModelDto? ChatbotModelSelectedInCurrentChatSession
    {
        get => _chatbotModelSelectedInCurrentChatSession;
        set => _chatbotModelSelectedInCurrentChatSession = value;
    }

    public ChatbotDefaultConfigurationDto? ChatbotDefaultConfigurationSelected => _chatbotDefaultConfigurationSelected;

    public ChatSessionDefaultConfigurationDto? ChatSessionDefaultConfiguration => _chatSessionDefaultConfiguration;

    public bool SettingsPageOpen => _settingsPageOpen;

    public string SettingsMenuSelected => _settingsMenuSelected;

    public ChatbotDto? GetChatbot(string? chatbotName)
    {
        if (chatbotName is null)
        {
            return null;
        }
        
        return _chatbots?.Find(c => c.Name == chatbotName);
    }

    public ChatbotModelDto? GetTextGenerationChatbotModel(ChatbotDto? chatbot, string? chatbotModel)
    {
        if (chatbot is null || chatbotModel is null)
        {
            return null;
        }

        return chatbot.TextGenerationChatbotModels.Find(tgm => tgm.Name == chatbotModel);
    }
    
    public ChatbotModelDto? GetTextGenerationChatbotModel(string? chatbotName, string? chatbotModel)
    {
        if (chatbotName is null || chatbotModel is null)
        {
            return null;
        }

        var chatbot = _chatbots?.Find(c => c.Name == chatbotName);
        
        return chatbot?.TextGenerationChatbotModels.Find(tgm => tgm.Name == chatbotModel);
    }
    
    public ChatbotModelDto? GetImageGenerationChatbotModel(ChatbotDto? chatbot, string? chatbotModel)
    {
        if (chatbot is null || chatbotModel is null)
        {
            return null;
        }

        return chatbot.ImageGenerationChatbotModels.Find(tgm => tgm.Name == chatbotModel);
    }
    
    public ChatbotModelDto? GetImageGenerationChatbotModel(string? chatbotName, string? chatbotModel)
    {
        if (chatbotName is null || chatbotModel is null)
        {
            return null;
        }

        var chatbot = _chatbots?.Find(c => c.Name == chatbotName);
        
        return chatbot?.ImageGenerationChatbotModels.Find(tgm => tgm.Name == chatbotModel);
    }

    public ChatbotDefaultConfigurationDto? GetChatbotDefaultConfiguration(string? chatbotName)
    {
        if (chatbotName is null)
        {
            return null;
        }

        return _chatbotDefaultConfigurations?.Find(cdc => cdc.ChatbotName == chatbotName);
    }

    public async Task SetUpConfiguration()
    {
        await LoadChatbots();
        await LoadChatbotDefaultConfigurations();
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

    public async Task SaveChatbotDefaultSettings(ChatbotDefaultSettings chatbotDefaultSettings)
    {
        if (_userStateService.UserLogged is null)
        {
            return;
        }
        
        var chatbotDefaultConfigurationDto = new ChatbotDefaultConfigurationDto
        {
            ChatbotName = chatbotDefaultSettings.ChatbotName,
            TextGenerationChatbotModel = chatbotDefaultSettings.TextGenerationChatbotModel,
            ImageGenerationChatbotModel = chatbotDefaultSettings.ImageGenerationChatbotModel,
            ApiHost = chatbotDefaultSettings.ApiHost,
            ApiKey = chatbotDefaultSettings.ApiKey,
            Temperature = chatbotDefaultSettings.Temperature
        };

        var chatbotDefaultConfiguration = GetChatbotDefaultConfiguration(chatbotDefaultSettings.ChatbotName);

        if (chatbotDefaultConfiguration is not null)
        {
            chatbotDefaultConfiguration.ChatbotName = chatbotDefaultConfigurationDto.ChatbotName;
            chatbotDefaultConfiguration.TextGenerationChatbotModel =
                chatbotDefaultConfigurationDto.TextGenerationChatbotModel;
            chatbotDefaultConfiguration.ImageGenerationChatbotModel =
                chatbotDefaultConfigurationDto.ImageGenerationChatbotModel;
            chatbotDefaultConfiguration.ApiHost = chatbotDefaultConfigurationDto.ApiHost;
            chatbotDefaultConfiguration.ApiKey = chatbotDefaultConfigurationDto.ApiKey;
            chatbotDefaultConfiguration.Temperature = chatbotDefaultConfigurationDto.Temperature;

            _chatbotDefaultConfigurationSelected = chatbotDefaultConfiguration;
            
            await HttpClient.PutAsJsonAsync(
                $"configuration/{_userStateService.UserLogged.Id}/chatbot", chatbotDefaultConfigurationDto);
        }
    }

    public async Task SaveChatSessionDefaultSettings(ChatSessionDefaultSettings chatSessionDefaultSettings)
    {
        if (_userStateService.UserLogged is null)
        {
            return;
        }
        
        var chatSessionDefaultConfiguration = new ChatSessionDefaultConfigurationDto()
        {
            SystemInstruction = chatSessionDefaultSettings.SystemInstruction,
            TextStream = chatSessionDefaultSettings.TextStream,
            ImageSize = chatSessionDefaultSettings.ImageSize,
            ImagesToGenerate = chatSessionDefaultSettings.ImagesToGenerate
        };

        _chatSessionDefaultConfiguration = chatSessionDefaultConfiguration;
        
        await HttpClient.PutAsJsonAsync(
            $"configuration/{_userStateService.UserLogged.Id}/chat-session/chat-session", chatSessionDefaultConfiguration);
    }

    public void Logout()
    {
        _chatbotDefaultConfigurationSelected = null;
        _chatSessionDefaultConfiguration = null;

        _chatbotDefaultConfigurations = null;

        _chatbots = null;
    }

    private async Task LoadChatbots()
    {
        if (_chatbots is not null)
        {
            return;
        }
        
        var chatbots = await HttpClient.GetFromJsonAsync<List<ChatbotDto>>("chatbot");

        if (chatbots is null || chatbots.Count == 0)
        {
            NotifyNavigateToErrorPage("Error occured while loading the chatbots", 
                "No chatbot found");
            return;
        }

        _chatbots = chatbots;
    }
    
    private async Task LoadChatbotDefaultConfigurations()
    {
        if (_chatbotDefaultConfigurations is not null)
        {
            return;
        }

        if (_userStateService.UserLogged is null)
        {
            return;
        }

        var chatbotDefaultConfigurations = 
            await HttpClient.GetFromJsonAsync<List<ChatbotDefaultConfigurationDto>>(
                $"configuration/{_userStateService.UserLogged.Id}/chatbot");

        if (chatbotDefaultConfigurations is null || chatbotDefaultConfigurations.Count == 0)
        {
            NotifyNavigateToErrorPage("Error occured while loading the chatbot default configuration", 
                "No chatbot configuration found");
            return;
        }

        var chatbotDefaultConfigurationSelected = chatbotDefaultConfigurations
            .Find(cdc => cdc.Selected);

        if (chatbotDefaultConfigurationSelected is null)
        {
            chatbotDefaultConfigurationSelected = chatbotDefaultConfigurations.ElementAt(0);
        }

        _chatbotDefaultConfigurations = chatbotDefaultConfigurations;
        _chatbotDefaultConfigurationSelected = chatbotDefaultConfigurationSelected;
    }

    private async Task LoadChatSessionDefaultConfiguration()
    {
        if (_userStateService.UserLogged is null)
        {
            return;
        }
        
        var chatSessionDefaultConfigurationDto = 
            await HttpClient.GetFromJsonAsync<ChatSessionDefaultConfigurationDto>(
                $"configuration/{_userStateService.UserLogged.Id}/chat-session");

        if (chatSessionDefaultConfigurationDto is null)
        {
            NotifyNavigateToErrorPage("Error occured while loading the chat session configuration", 
                "No configuration found");
            return;
        }

        _chatSessionDefaultConfiguration = chatSessionDefaultConfigurationDto;
    }
}