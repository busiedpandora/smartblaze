using SmartBlaze.Backend.Dtos;
using SmartBlaze.Backend.Models;

namespace SmartBlaze.Backend.Services;

public class ChatbotService
{
    private HttpClient _httpClient;
    
    private List<Chatbot> _chatbots;

    public ChatbotService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _chatbots = new List<Chatbot>();
        
        CreateChatbots();
    }

    public List<Chatbot> GetAllChatbots()
    {
        return _chatbots;
    }

    public Chatbot? GetChatbotByName(string name)
    {
        return _chatbots.Find(c => c.Name == name);
    }

    public async Task<string?> GenerateTextInChatSession(Chatbot chatbot, ChatSessionDto chatSessionDto,
        List<MessageDto> messageDtos, string apiHost, string apiKey)
    {
        return await chatbot.GenerateText(chatSessionDto, messageDtos, apiHost, apiKey, _httpClient);
    }
    
    public async IAsyncEnumerable<string> GenerateTextStreamInChatSession(Chatbot chatbot, ChatSessionDto chatSessionDto,
        List<MessageDto> messageDtos, string apiHost, string apiKey)
    {
        await foreach (var chunk in chatbot.GenerateTextStreamEnabled(chatSessionDto, messageDtos, apiHost, apiKey, _httpClient))
        {
            yield return chunk;
        }
    }
    
    public void CreateChatbots()
    {
        var chatGpt = new ChatGpt("ChatGPT", ["gpt-4o", "gpt-4-turbo", "gpt-4", "gpt-3.5-turbo"]);
        _chatbots.Add(chatGpt);
        
        var gemini = new Gemini("Google Gemini", ["gemini-1.5-pro", "gemini-1.5-flash", "gemini-1.0-pro"]);
        _chatbots.Add(gemini);
    }
}