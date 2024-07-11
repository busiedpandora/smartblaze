using System.Text.Json;
using SmartBlaze.Frontend.Dtos;

namespace SmartBlaze.Frontend.Services;

public class SettingsService(IHttpClientFactory httpClientFactory) : AbstractService(httpClientFactory)
{
    private List<ChatbotDto>? _chatbots;
    private ChatbotDto? _chatbotSelected;
    private string? _chatbotSelectedModel;
    
    
    public List<ChatbotDto>? Chatbots => _chatbots;

    public ChatbotDto? ChatbotSelected => _chatbotSelected;

    public string? ChatbotSelectedModel => _chatbotSelectedModel;

    public void SelectChatbot(ChatbotDto chatbot)
    {
        if (_chatbots is null)
        {
            return;
        }

        if (_chatbotSelected is not null && _chatbotSelected.Name == chatbot.Name)
        {
            return;
        }

        if (chatbot.Models is null || chatbot.Models.Count == 0)
        {
            NotifyNavigateToErrorPage($"Error occured while selecting the chatbot {chatbot.Name}", 
                $"No models for chatbot {chatbot.Name} found");
            return;
        }

        _chatbotSelected = chatbot;
        _chatbotSelectedModel = chatbot.Models.ElementAt(0);
        
        NotifyRefreshView();
    }
    
    public async Task LoadChatbots()
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

        _chatbots = chatbots;

        if (_chatbots.Count > 0)
        {
            SelectChatbot(_chatbots.ElementAt(0));
        }
    }
}