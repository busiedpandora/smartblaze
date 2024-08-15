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

    public Chatbot GetChatbotSelected()
    {
        return _chatbots
            .Find(c => c.GetDefaultConfiguration().Selected) ?? _chatbots.First();
    }

    public async Task<AssistantMessageInfoDto> GenerateTextInChatSession(Chatbot chatbot, TextGenerationRequestData textGenerationRequestData)
    {
        return await chatbot.GenerateText(textGenerationRequestData, _httpClient);
    }
    
    public async IAsyncEnumerable<string> GenerateTextStreamInChatSession(Chatbot chatbot, TextGenerationRequestData textGenerationRequestData)
    {
        await foreach (var chunk in chatbot.GenerateTextStreamEnabled(textGenerationRequestData, _httpClient))
        {
            yield return chunk;
        }
    }

    public async Task<AssistantMessageInfoDto> GenerateImageInChatSession(Chatbot chatbot, ImageGenerationRequestData imageGenerationRequestData)
    {
        return await chatbot.GenerateImage(imageGenerationRequestData, _httpClient);
    }
    
    private void CreateChatbots()
    {
        var chatGpt = new ChatGpt("ChatGPT",
            [
                new TextGenerationChatbotModel("gpt-4o", true, true, 
                0.0f, 2.0f, true, true, 
                true, 100, true),
                new TextGenerationChatbotModel("gpt-4-turbo", true, true, 
                    0.0f, 2.0f, true, true, 
                    true, 100, true),
                new TextGenerationChatbotModel("gpt-4", true, true, 
                    0.0f, 2.0f, true, true, 
                    true, 100, true),
                new TextGenerationChatbotModel("gpt-3.5-turbo", true, true, 
                    0.0f, 2.0f, true, true, 
                    true, 100, true)
            ],
            [
                new ImageGenerationChatbotModel("dall-e-3", false, 
                    false, true, ["1024x1024", "1024x1792", "1792x1024"], 
                    false, 1),
                new ImageGenerationChatbotModel("dall-e-2", false, 
                    false, false, ["1024x1024"], 
                    true, 10)
            ]);
        
        _chatbots.Add(chatGpt);
        
        
        var gemini = new Gemini("Google Gemini",
            [
                new TextGenerationChatbotModel("gemini-1.5-pro", true, true, 
                    0.0f, 2.0f, true, true, 
                    true, 400, true),
                new TextGenerationChatbotModel("gemini-1.5-flash", true, true, 
                    0.0f, 2.0f, true, true, 
                    true, 400, true),
            ],
            []
        );
        
        _chatbots.Add(gemini);
    }
}