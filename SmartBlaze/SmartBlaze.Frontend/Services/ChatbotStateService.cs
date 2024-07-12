using System.Text.Json;
using SmartBlaze.Frontend.Dtos;

namespace SmartBlaze.Frontend.Services;

public class ChatbotStateService(IHttpClientFactory httpClientFactory) : AbstractService(httpClientFactory)
{
    private List<ChatbotDto>? _chatbots;
    private ChatbotDto? _chatbotSelected;
    private string? _chatbotSelectedModel;
    
    
    public List<ChatbotDto>? Chatbots => _chatbots;

    public ChatbotDto? ChatbotSelected => _chatbotSelected;

    public string? ChatbotSelectedModel => _chatbotSelectedModel;

    public void SelectChatbot(ChatbotDto chatbot, string chatbotModel)
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