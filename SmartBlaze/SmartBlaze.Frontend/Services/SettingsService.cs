using System.Text.Json;
using SmartBlaze.Frontend.Dtos;
using SmartBlaze.Frontend.Models;

namespace SmartBlaze.Frontend.Services;

public class SettingsService(IHttpClientFactory httpClientFactory) : AbstractService(httpClientFactory)
{
    private List<ChatbotDto>? _chatbots;
    private ChatbotDto? _chatbotSelected;
    private string _chatbotSelectedModel = "";

    private string _systemInstruction = "";
    private bool _textStream;
    
    private bool _settingsPageOpen = false;
    private string _settingsMenuSelected = "model";
    
    
    public bool SettingsPageOpen
    {
        get => _settingsPageOpen;
    }

    public string SettingsMenuSelected => _settingsMenuSelected;
    
    public List<ChatbotDto>? Chatbots => _chatbots;

    public ChatbotDto? ChatbotSelected => _chatbotSelected;

    public string ChatbotSelectedModel => _chatbotSelectedModel;

    public string SystemInstruction => _systemInstruction;

    public bool TextStream => _textStream;

    public async Task SetUpConfiguration()
    {
        await LoadChatbots();

        _systemInstruction = "You are a helpful assistant. You can help me by answering my questions.";
        _textStream = true;
    }
    
    public void OpenModelsSettings()
    {
        _settingsPageOpen = true;
        _settingsMenuSelected = "model";
        
        NotifyNavigateToPage("/settings/models");
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

    public void SaveModelSettings(ModelSettings modelSettings)
    {
        var chatbot = _chatbots?.Find(c => c.Name == modelSettings.ChatbotName);

        if (chatbot is not null)
        {
            SelectChatbot(chatbot, modelSettings.ChatbotModel);
        }
    }

    public void SaveChatSessionSettings(ChatSessionSettings chatSessionSettings)
    {
        _systemInstruction = chatSessionSettings.SystemInstruction;
        _textStream = chatSessionSettings.TextStream;
    }
    
    private void SelectChatbot(ChatbotDto chatbot, string chatbotModel)
    {
        if (_chatbots is null)
        {
            return;
        }

        if (chatbot.Models is null || chatbot.Models.Count == 0)
        {
            NotifyNavigateToErrorPage($"Error occured while selecting the chatbot {chatbot.Name}", 
                $"No models for chatbot {chatbot.Name} found");
            NotifyRefreshView();
            return;
        }

        _chatbotSelected = chatbot;
        _chatbotSelectedModel = chatbotModel;
    }
    
    private async Task LoadChatbots()
    {
        if (_chatbots is not null)
        {
            return;
        }

        var chatbotsResponse = await HttpClient.GetAsync("chatbots");
        var chatbotsResponseContent = await chatbotsResponse.Content.ReadAsStringAsync();

        if (!chatbotsResponse.IsSuccessStatusCode)
        {
            NotifyNavigateToErrorPage("Error occured while loading the chatbots", chatbotsResponseContent);
            return;
        }

        var chatbots = JsonSerializer.Deserialize<List<ChatbotDto>>(chatbotsResponseContent) ?? new List<ChatbotDto>();

        if (chatbots.Count == 0)
        {
            NotifyNavigateToErrorPage("Error occured while loading the chatbots", "No chatbot found");
            return;
        }
        
        _chatbots = chatbots;
        
        var chatbotToSelect = _chatbots.ElementAt(0);

        if (chatbotToSelect.Models is null || chatbotToSelect.Models.Count == 0)
        {
            NotifyNavigateToErrorPage("Error occured while loading the chatbots", 
                $"No model found for chatbot {chatbotToSelect.Name}");
            return;
        }
        
        SelectChatbot(chatbotToSelect, chatbotToSelect.Models.ElementAt(0));
    }
}