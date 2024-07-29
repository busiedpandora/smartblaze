using Appwrite;
using Appwrite.Models;
using SmartBlaze.Backend.Dtos;

namespace SmartBlaze.Backend.Repositories;

public class ConfigurationRepository : AbstractRepository
{
    public async Task<ChatbotDefaultConfigurationDto?> GetChatbotDefaultConfiguration(string chatbotName)
    {
        var chatbotDefaultConfigurationDocuments = await AppwriteDatabase.ListDocuments(AppwriteDatabaseId,
            ChatbotDefaultConfigurationCollectionId,
            [
                Query.Equal("chatbotName", chatbotName)
            ]);

        var chatbotDefaultConfigurationDocument = chatbotDefaultConfigurationDocuments.Documents.Count > 0 
            ? chatbotDefaultConfigurationDocuments.Documents.First() : null;

        if (chatbotDefaultConfigurationDocument is null)
        {
            return null;
        }
        
        var chatbotDefaultConfiguration = ConvertToChatbotDefaultConfiguration(chatbotDefaultConfigurationDocument);

        return chatbotDefaultConfiguration;
    }
    
    public async Task<ChatbotDefaultConfigurationDto?> GetSelectedChatbotDefaultConfiguration()
    {
        var chatbotDefaultConfigurationDocuments = await AppwriteDatabase.ListDocuments(AppwriteDatabaseId,
            ChatbotDefaultConfigurationCollectionId,
            [
                Query.Equal("selected", true)
            ]);
        
        var selectedChatbotDefaultConfigurationDocument = chatbotDefaultConfigurationDocuments.Documents.Count > 0 
            ? chatbotDefaultConfigurationDocuments.Documents.First() : null;

        if (selectedChatbotDefaultConfigurationDocument is null)
        {
            return null;
        }

        var selectedChatbotDefaultConfiguration = ConvertToChatbotDefaultConfiguration(selectedChatbotDefaultConfigurationDocument);

        return selectedChatbotDefaultConfiguration;
    }
    
    public async Task SaveChatbotDefaultConfiguration(ChatbotDefaultConfigurationDto chatbotDefaultConfigurationDto)
    {
        var chatbotDefaultConfigurationDocument = new Dictionary<string, object>()
        {
            { "chatbotName", chatbotDefaultConfigurationDto.ChatbotName ?? ""},
            { "chatbotModel", chatbotDefaultConfigurationDto.ChatbotModel ?? ""},
            { "apiHost", chatbotDefaultConfigurationDto.ApiHost ?? ""},
            { "apiKey", chatbotDefaultConfigurationDto.ApiKey ?? ""},
            { "selected", chatbotDefaultConfigurationDto.Selected },
            { "textStreamDelay", chatbotDefaultConfigurationDto.TextStreamDelay },
            { "temperature" , chatbotDefaultConfigurationDto.Temperature },
            { "minTemperature", chatbotDefaultConfigurationDto.MinTemperature },
            { "maxTemperature", chatbotDefaultConfigurationDto.MaxTemperature }
        };

        await AppwriteDatabase.CreateDocument(AppwriteDatabaseId, ChatbotDefaultConfigurationCollectionId, 
            ID.Unique(), chatbotDefaultConfigurationDocument);
    }
    
    public async Task EditChatbotDefaultConfiguration(ChatbotDefaultConfigurationDto chatbotDefaultConfigurationDto)
    {
        if (string.IsNullOrEmpty(chatbotDefaultConfigurationDto.Id))
        {
            throw new ArgumentException("The document ID must be provided.", nameof(chatbotDefaultConfigurationDto.Id));
        }
        
        var chatbotConfigurationDocument = new Dictionary<string, object>()
        {
            { "chatbotName", chatbotDefaultConfigurationDto.ChatbotName ?? ""},
            { "chatbotModel", chatbotDefaultConfigurationDto.ChatbotModel ?? ""},
            { "apiHost", chatbotDefaultConfigurationDto.ApiHost ?? ""},
            { "apiKey", chatbotDefaultConfigurationDto.ApiKey ?? ""},
            { "selected", chatbotDefaultConfigurationDto.Selected},
            { "textStreamDelay", chatbotDefaultConfigurationDto.TextStreamDelay },
            { "temperature" , chatbotDefaultConfigurationDto.Temperature },
            { "minTemperature", chatbotDefaultConfigurationDto.MinTemperature },
            { "maxTemperature", chatbotDefaultConfigurationDto.MaxTemperature }
        };
        
        await AppwriteDatabase.UpdateDocument(AppwriteDatabaseId, ChatbotDefaultConfigurationCollectionId, 
            chatbotDefaultConfigurationDto.Id, chatbotConfigurationDocument);
    }
    
    public async Task<ChatSessionDefaultConfigurationDto?> GetChatSessionDefaultConfiguration()
    {
        var chatSessionDefaultConfigurationDocuments =
            await AppwriteDatabase.ListDocuments(AppwriteDatabaseId, ChatSessionDefaultConfigurationCollectionId);

        var chatSessionDefaultConfigurationDocument =
            chatSessionDefaultConfigurationDocuments.Documents.Count > 0 ? chatSessionDefaultConfigurationDocuments.Documents.First() : null;

        if (chatSessionDefaultConfigurationDocument is null)
        {
            return null;
        }

        var chatSessionConfiguration = ConvertToChatSessionDefaultConfiguration(chatSessionDefaultConfigurationDocument);

        return chatSessionConfiguration;
    }
    
    public async Task SaveChatSessionDefaultConfiguration(ChatSessionDefaultConfigurationDto chatSessionDefaultConfigurationDto)
    {
        var chatSessionConfigurationDocument = new Dictionary<string, object>()
        {
            { "systemInstruction", chatSessionDefaultConfigurationDto.SystemInstruction ?? "" },
            { "textStream", chatSessionDefaultConfigurationDto.TextStream}
        };

        await AppwriteDatabase.CreateDocument(AppwriteDatabaseId, ChatSessionDefaultConfigurationCollectionId,
            ID.Unique(), chatSessionConfigurationDocument);
    }
    
    public async Task EditChatSessionDefaultConfiguration(ChatSessionDefaultConfigurationDto chatSessionDefaultConfigurationDto)
    {
        if (string.IsNullOrEmpty(chatSessionDefaultConfigurationDto.Id))
        {
            throw new ArgumentException("The document ID must be provided.", nameof(chatSessionDefaultConfigurationDto.Id));
        }
        
        var chatSessionConfigurationDocument = new Dictionary<string, object>()
        {
            { "systemInstruction", chatSessionDefaultConfigurationDto.SystemInstruction ?? "" },
            { "textStream", chatSessionDefaultConfigurationDto.TextStream}
        };

        await AppwriteDatabase.UpdateDocument(AppwriteDatabaseId, ChatSessionDefaultConfigurationCollectionId,
            chatSessionDefaultConfigurationDto.Id, chatSessionConfigurationDocument);
    }

    private ChatbotDefaultConfigurationDto ConvertToChatbotDefaultConfiguration(
        Document chatbotDefaultConfigurationDocument)
    {
        var chatbotDefaultConfigurationDto = new ChatbotDefaultConfigurationDto()
        {
            Id = chatbotDefaultConfigurationDocument.Id,
            ChatbotName = chatbotDefaultConfigurationDocument.Data["chatbotName"].ToString(),
            ChatbotModel = chatbotDefaultConfigurationDocument.Data["chatbotModel"].ToString(),
            ApiHost = chatbotDefaultConfigurationDocument.Data["apiHost"].ToString(),
            ApiKey = chatbotDefaultConfigurationDocument.Data["apiKey"].ToString(),
            Selected = bool.Parse(chatbotDefaultConfigurationDocument.Data["selected"].ToString() ?? "false"),
            TextStreamDelay = int.Parse(chatbotDefaultConfigurationDocument.Data["textStreamDelay"].ToString() ?? "100"),
            Temperature = float.Parse(chatbotDefaultConfigurationDocument.Data["temperature"].ToString() ?? "0.0"),
            MinTemperature = float.Parse(chatbotDefaultConfigurationDocument.Data["minTemperature"].ToString() ?? "0.0"),
            MaxTemperature = float.Parse(chatbotDefaultConfigurationDocument.Data["maxTemperature"].ToString() ?? "0.0")
        };

        return chatbotDefaultConfigurationDto;
    }

    private ChatSessionDefaultConfigurationDto ConvertToChatSessionDefaultConfiguration(
        Document chatSessionDefaultConfigurationDocument)
    {
        var chatSessionDefaultConfigurationDto = new ChatSessionDefaultConfigurationDto()
        {
            Id = chatSessionDefaultConfigurationDocument.Id,
            SystemInstruction = chatSessionDefaultConfigurationDocument.Data["systemInstruction"].ToString(),
            TextStream = bool.Parse(chatSessionDefaultConfigurationDocument.Data["textStream"].ToString() ?? "false")
        };

        return chatSessionDefaultConfigurationDto;
    }
}