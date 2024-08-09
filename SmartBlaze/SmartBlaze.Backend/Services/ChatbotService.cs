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

    public async Task<string?> GenerateTextInChatSession(Chatbot chatbot, ChatSessionInfoDto chatSessionInfoDto)
    {
        return await chatbot.GenerateText(chatSessionInfoDto, _httpClient);
    }
    
    public async IAsyncEnumerable<string> GenerateTextStreamInChatSession(Chatbot chatbot, ChatSessionInfoDto chatSessionInfoDto)
    {
        await foreach (var chunk in chatbot.GenerateTextStreamEnabled(chatSessionInfoDto, _httpClient))
        {
            yield return chunk;
        }
    }

    public async Task<string> UploadFileFromMessage(Chatbot chatbot, string fileName, byte[] data, ChatSessionInfoDto chatSessionInfoDto)
    {
        return await chatbot.UploadFile(fileName, data, chatSessionInfoDto, _httpClient);
    }
    
    private void CreateChatbots()
    {
        var chatGpt = new ChatGpt("ChatGPT", ["gpt-4o", "gpt-4-turbo", "gpt-4", "gpt-3.5-turbo"]);
        _chatbots.Add(chatGpt);
        
        var gemini = new Gemini("Google Gemini", ["gemini-1.5-pro", "gemini-1.5-flash"]);
        _chatbots.Add(gemini);
    }
}