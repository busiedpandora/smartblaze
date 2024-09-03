using SmartBlaze.Backend.Dtos;
using SmartBlaze.Backend.Models;

namespace SmartBlaze.Backend.Services;

public class ChatbotService : IChatbotService
{
    private readonly HttpClient _httpClient;
    
    private readonly List<Chatbot> _chatbots;

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

    public async Task<AssistantMessageInfoDto> EntitleChatSessionFromUserMessage(Chatbot chatbot,
        TextGenerationRequestData textGenerationRequestData)
    {
        return await chatbot.EntitleChatSession(textGenerationRequestData, _httpClient);
    }
    
    private void CreateChatbots()
    {
        var chatGpt = new ChatGpt("ChatGPT",
            [
                new TextGenerationChatbotModel("gpt-4o", true, true, true, 
                true, true, 0.0f, 2.0f, 
                true, 100, true),
                
                new TextGenerationChatbotModel("gpt-4o-2024-08-06", true, true, true, 
                    true, true, 0.0f, 2.0f, 
                    true, 100, true),
                
                new TextGenerationChatbotModel("chatgpt-4o-latest", true, true, true, 
                    true, true, 0.0f, 2.0f, 
                    true, 100, true),
                
                new TextGenerationChatbotModel("gpt-4o-mini", true, true, true, 
                    true, true, 0.0f, 2.0f, 
                    true, 100, true),
                
                new TextGenerationChatbotModel("gpt-4-turbo", true, true, true, 
                    true, true, 0.0f, 2.0f, 
                    true, 100, true),
                
                new TextGenerationChatbotModel("gpt-4-turbo-preview", true, true, true, 
                    true, true, 0.0f, 2.0f, 
                    true, 100, true),
                
                new TextGenerationChatbotModel("gpt-4-1106-preview", true, true, true, 
                    true, true, 0.0f, 2.0f, 
                    true, 100, true),
                
                new TextGenerationChatbotModel("gpt-4", true, true, true, 
                    true, true, 0.0f, 2.0f, 
                    true, 100, true),
                
                new TextGenerationChatbotModel("gpt-4-0314", true, true, true, 
                    true, true, 0.0f, 2.0f, 
                    true, 100, true),
                
                new TextGenerationChatbotModel("gpt-3.5-turbo", true, true, true, 
                    true, true, 0.0f, 2.0f, 
                    true, 100, true),
                
                new TextGenerationChatbotModel("gpt-3.5-turbo-1106", true, true, true, 
                    true, true, 0.0f, 2.0f, 
                    true, 100, true),
            ],
            [
                new ImageGenerationChatbotModel("dall-e-3", false, 
                    false, false, true, ["1024x1024", "1024x1792", "1792x1024"], 
                    false, 1),
                
                new ImageGenerationChatbotModel("dall-e-2", false, 
                    false, false, false, [], 
                    true, 10)
            ],
            true);
        
        _chatbots.Add(chatGpt);
        
        
        var gemini = new Gemini("Google Gemini",
            [
                new TextGenerationChatbotModel("gemini-1.5-flash", true, false, 
                    true, true, true, 0.0f, 2.0f, 
                    true, 400, true),
                
                new TextGenerationChatbotModel("gemini-1.5-flash-latest", true, false, 
                    true, true, true, 0.0f, 2.0f, 
                    true, 400, true),
                
                new TextGenerationChatbotModel("gemini-1.5-flash-001", true, false, 
                    true, true, true, 0.0f, 2.0f, 
                    true, 400, true),
                
                new TextGenerationChatbotModel("gemini-1.5-flash-exp-0827", true, false, 
                    true, true, true, 0.0f, 2.0f, 
                    true, 400, true),
                
                new TextGenerationChatbotModel("gemini-1.5-flash-8b-exp-0827", true, false, 
                    true, true, true, 0.0f, 2.0f, 
                    true, 400, true),
                
                new TextGenerationChatbotModel("gemini-1.5-pro", true, false, 
                    true, true, true, 0.0f, 2.0f, 
                    true, 400, true),
                
                new TextGenerationChatbotModel("gemini-1.5-pro-latest", true, false, 
                    true, true, true, 0.0f, 2.0f, 
                    true, 400, true),
                
                new TextGenerationChatbotModel("gemini-1.5-pro-001", true, false, 
                    true, true, true, 0.0f, 2.0f, 
                    true, 400, true),
                
                new TextGenerationChatbotModel("gemini-1.5-pro-exp-0801", true, false, 
                    true, true, true, 0.0f, 2.0f, 
                    true, 400, true),
                
                new TextGenerationChatbotModel("gemini-1.5-pro-exp-0827", true, false, 
                    true, true, true, 0.0f, 2.0f, 
                    true, 400, true),
            ],
            [],
            false
        );
        
        _chatbots.Add(gemini);
    }
}