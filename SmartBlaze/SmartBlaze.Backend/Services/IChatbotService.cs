using SmartBlaze.Backend.Dtos;
using SmartBlaze.Backend.Models;

namespace SmartBlaze.Backend.Services;

public interface IChatbotService
{
    List<Chatbot> GetAllChatbots();
    Chatbot? GetChatbotByName(string name);

    Task<AssistantMessageInfoDto> GenerateTextInChatSession(Chatbot chatbot,
        TextGenerationRequestData textGenerationRequestData);

    IAsyncEnumerable<string> GenerateTextStreamInChatSession(Chatbot chatbot,
        TextGenerationRequestData textGenerationRequestData);

    Task<AssistantMessageInfoDto> GenerateImageInChatSession(Chatbot chatbot,
        ImageGenerationRequestData imageGenerationRequestData);

    Task<AssistantMessageInfoDto> EntitleChatSessionFromUserMessage(Chatbot chatbot,
        TextGenerationRequestData textGenerationRequestData);
    
}