using Appwrite;
using Appwrite.Models;
using SmartBlaze.Backend.Dtos;

namespace SmartBlaze.Backend.Repositories;

public class ConfigurationRepository : AbstractRepository
{
    public async Task<List<ChatbotConfigurationDto>> GetAllChatbotConfigurations()
    {
        var chatbotConfigurationDocuments =
            await AppwriteDatabase.ListDocuments(AppwriteDatabaseId, ChatbotConfigurationCollectionId);

        var chatbotConfigurationDtos = chatbotConfigurationDocuments.Documents
            .Select(ConvertToChatbotConfiguration)
            .ToList();

        return chatbotConfigurationDtos;
    }

    public async Task<ChatbotConfigurationDto?> GetChatbotConfiguration(string chatbotName)
    {
        var chatbotConfigurationDocuments = await AppwriteDatabase.ListDocuments(AppwriteDatabaseId,
            ChatbotConfigurationCollectionId,
            [
                Query.Equal("chatbotName", chatbotName)
            ]);

        var chatbotConfigurationDocument = chatbotConfigurationDocuments.Documents.Count > 0 
            ? chatbotConfigurationDocuments.Documents.First() : null;

        if (chatbotConfigurationDocument is null)
        {
            return null;
        }

        var chatbotConfiguration = ConvertToChatbotConfiguration(chatbotConfigurationDocument);

        return chatbotConfiguration;
    }

    public async Task<ChatbotConfigurationDto?> GetSelectedChatbotConfiguration()
    {
        var chatbotConfigurationDocuments = await AppwriteDatabase.ListDocuments(AppwriteDatabaseId,
            ChatbotConfigurationCollectionId,
            [
                Query.Equal("selected", true)
            ]);
        
        var selectedChatbotConfigurationDocument = chatbotConfigurationDocuments.Documents.Count > 0 
            ? chatbotConfigurationDocuments.Documents.First() : null;

        if (selectedChatbotConfigurationDocument is null)
        {
            return null;
        }

        var selectedChatbotConfiguration = ConvertToChatbotConfiguration(selectedChatbotConfigurationDocument);

        return selectedChatbotConfiguration;
    }

    public async Task SaveChatbotConfiguration(ChatbotConfigurationDto chatbotConfigurationDto)
    {
        var chatbotConfigurationDocument = new Dictionary<string, object>()
        {
            { "chatbotName", chatbotConfigurationDto.ChatbotName ?? ""},
            { "chatbotModel", chatbotConfigurationDto.ChatbotModel ?? ""},
            { "apiHost", chatbotConfigurationDto.ApiHost ?? ""},
            { "apiKey", chatbotConfigurationDto.ApiKey ?? ""},
            { "selected", chatbotConfigurationDto.Selected },
            { "textStreamDelay", chatbotConfigurationDto.TextStreamDelay }
        };

        await AppwriteDatabase.CreateDocument(AppwriteDatabaseId, ChatbotConfigurationCollectionId, 
            ID.Unique(), chatbotConfigurationDocument);
    }

    public async Task EditChatbotConfiguration(ChatbotConfigurationDto chatbotConfigurationDto)
    {
        if (string.IsNullOrEmpty(chatbotConfigurationDto.Id))
        {
            throw new ArgumentException("The document ID must be provided.", nameof(chatbotConfigurationDto.Id));
        }
        
        var chatbotConfigurationDocument = new Dictionary<string, object>()
        {
            { "chatbotName", chatbotConfigurationDto.ChatbotName ?? ""},
            { "chatbotModel", chatbotConfigurationDto.ChatbotModel ?? ""},
            { "apiHost", chatbotConfigurationDto.ApiHost ?? ""},
            { "apiKey", chatbotConfigurationDto.ApiKey ?? ""},
            { "selected", chatbotConfigurationDto.Selected},
            { "textStreamDelay", chatbotConfigurationDto.TextStreamDelay }
        };
        
        await AppwriteDatabase.UpdateDocument(AppwriteDatabaseId, ChatbotConfigurationCollectionId, 
            chatbotConfigurationDto.Id, chatbotConfigurationDocument);
    }

    public async Task<ChatSessionConfigurationDto?> GetChatSessionConfiguration()
    {
        var chatSessionConfigurationDocuments =
            await AppwriteDatabase.ListDocuments(AppwriteDatabaseId, ChatSessionConfigurationCollectionId);

        var chatSessionConfigurationDocument =
            chatSessionConfigurationDocuments.Documents.Count > 0 ? chatSessionConfigurationDocuments.Documents.First() : null;

        if (chatSessionConfigurationDocument is null)
        {
            return null;
        }

        var chatSessionConfiguration = ConvertToChatSessionConfiguration(chatSessionConfigurationDocument);

        return chatSessionConfiguration;
    }

    public async Task SaveChatSessionConfiguration(ChatSessionConfigurationDto chatSessionConfigurationDto)
    {
        var chatSessionConfigurationDocument = new Dictionary<string, object>()
        {
            { "systemInstruction", chatSessionConfigurationDto.SystemInstruction ?? "" },
            { "textStream", chatSessionConfigurationDto.TextStream ?? false}
        };

        await AppwriteDatabase.CreateDocument(AppwriteDatabaseId, ChatSessionConfigurationCollectionId,
            ID.Unique(), chatSessionConfigurationDocument);
    }
    
    public async Task EditChatSessionConfiguration(ChatSessionConfigurationDto chatSessionConfigurationDto)
    {
        if (string.IsNullOrEmpty(chatSessionConfigurationDto.Id))
        {
            throw new ArgumentException("The document ID must be provided.", nameof(chatSessionConfigurationDto.Id));
        }
        
        var chatSessionConfigurationDocument = new Dictionary<string, object>()
        {
            { "systemInstruction", chatSessionConfigurationDto.SystemInstruction ?? "" },
            { "textStream", chatSessionConfigurationDto.TextStream ?? false}
        };

        await AppwriteDatabase.UpdateDocument(AppwriteDatabaseId, ChatSessionConfigurationCollectionId,
            chatSessionConfigurationDto.Id, chatSessionConfigurationDocument);
    }
        
    private ChatbotConfigurationDto ConvertToChatbotConfiguration(Document chatbotConfigurationDocument)
    {
        var chatbotConfigurationDto = new ChatbotConfigurationDto()
        {
            Id = chatbotConfigurationDocument.Id,
            ChatbotName = chatbotConfigurationDocument.Data["chatbotName"].ToString(),
            ChatbotModel = chatbotConfigurationDocument.Data["chatbotModel"].ToString(),
            ApiHost = chatbotConfigurationDocument.Data["apiHost"].ToString(),
            ApiKey = chatbotConfigurationDocument.Data["apiKey"].ToString(),
            Selected = bool.Parse(chatbotConfigurationDocument.Data["selected"].ToString() ?? "false"),
            TextStreamDelay = int.Parse(chatbotConfigurationDocument.Data["textStreamDelay"].ToString() ?? "100")
        };

        return chatbotConfigurationDto;
    }

    private ChatSessionConfigurationDto ConvertToChatSessionConfiguration(Document chatSessionConfigurationDocument)
    {
        var chatSessionConfigurationDto = new ChatSessionConfigurationDto()
        {
            Id = chatSessionConfigurationDocument.Id,
            SystemInstruction = chatSessionConfigurationDocument.Data["systemInstruction"].ToString(),
            TextStream = bool.Parse(chatSessionConfigurationDocument.Data["textStream"].ToString() ?? "false")
        };

        return chatSessionConfigurationDto;
    }
}