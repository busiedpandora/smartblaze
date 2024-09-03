using Appwrite;
using Appwrite.Models;
using SmartBlaze.Backend.Dtos;

namespace SmartBlaze.Backend.Repositories;

public class ConfigurationRepository : AbstractRepository, IConfigurationRepository
{
    public async Task<ChatbotDefaultConfigurationDto?> GetChatbotDefaultConfiguration(string chatbotName, string userId)
    {
        var chatbotDefaultConfigurationDocuments = await AppwriteDatabase.ListDocuments(AppwriteDatabaseId,
            ChatbotDefaultConfigurationCollectionId,
            [
                Query.Equal("user", userId),
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
    
    public async Task<ChatbotDefaultConfigurationDto?> GetSelectedChatbotDefaultConfiguration(string userId)
    {
        var chatbotDefaultConfigurationDocuments = await AppwriteDatabase.ListDocuments(AppwriteDatabaseId,
            ChatbotDefaultConfigurationCollectionId,
            [
                Query.Equal("user", userId),
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
    
    public async Task SaveChatbotDefaultConfiguration(ChatbotDefaultConfigurationDto chatbotDefaultConfigurationDto, string userId)
    {
        var chatbotDefaultConfigurationDocument = new Dictionary<string, object>()
        {
            { "chatbotName", chatbotDefaultConfigurationDto.ChatbotName ?? ""},
            { "textGenerationChatbotModel", chatbotDefaultConfigurationDto.TextGenerationChatbotModel ?? ""},
            { "imageGenerationChatbotModel", chatbotDefaultConfigurationDto.ImageGenerationChatbotModel ?? ""},
            { "apiHost", chatbotDefaultConfigurationDto.ApiHost ?? ""},
            { "apiKey", chatbotDefaultConfigurationDto.ApiKey ?? ""},
            { "temperature" , chatbotDefaultConfigurationDto.Temperature },
            { "selected", chatbotDefaultConfigurationDto.Selected},
            { "user", userId}
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
            { "textGenerationChatbotModel", chatbotDefaultConfigurationDto.TextGenerationChatbotModel ?? ""},
            { "imageGenerationChatbotModel", chatbotDefaultConfigurationDto.ImageGenerationChatbotModel ?? ""},
            { "apiHost", chatbotDefaultConfigurationDto.ApiHost ?? ""},
            { "apiKey", chatbotDefaultConfigurationDto.ApiKey ?? ""},
            { "temperature" , chatbotDefaultConfigurationDto.Temperature },
            { "selected", chatbotDefaultConfigurationDto.Selected}
        };
        
        await AppwriteDatabase.UpdateDocument(AppwriteDatabaseId, ChatbotDefaultConfigurationCollectionId, 
            chatbotDefaultConfigurationDto.Id, chatbotConfigurationDocument);
    }
    
    public async Task<ChatSessionDefaultConfigurationDto?> GetChatSessionDefaultConfiguration(string userId)
    {
        var chatSessionDefaultConfigurationDocuments =
            await AppwriteDatabase.ListDocuments(AppwriteDatabaseId, ChatSessionDefaultConfigurationCollectionId,
                [
                    Query.Equal("user", userId)
                ]);

        var chatSessionDefaultConfigurationDocument =
            chatSessionDefaultConfigurationDocuments.Documents.Count > 0 ? chatSessionDefaultConfigurationDocuments.Documents.First() : null;

        if (chatSessionDefaultConfigurationDocument is null)
        {
            return null;
        }

        var chatSessionDefaultConfiguration = ConvertToChatSessionDefaultConfiguration(chatSessionDefaultConfigurationDocument);

        return chatSessionDefaultConfiguration;
    }
    
    public async Task SaveChatSessionDefaultConfiguration(ChatSessionDefaultConfigurationDto chatSessionDefaultConfigurationDto,
        string userId)
    {
        var chatSessionDefaultConfigurationDocument = new Dictionary<string, object>()
        {
            { "systemInstruction", chatSessionDefaultConfigurationDto.SystemInstruction ?? "" },
            { "textStream", chatSessionDefaultConfigurationDto.TextStream},
            { "imageSize", chatSessionDefaultConfigurationDto.ImageSize ?? ""},
            { "imagesToGenerate", chatSessionDefaultConfigurationDto.ImagesToGenerate},
            { "user", userId}
        };

        await AppwriteDatabase.CreateDocument(AppwriteDatabaseId, ChatSessionDefaultConfigurationCollectionId,
            ID.Unique(), chatSessionDefaultConfigurationDocument);
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
            { "textStream", chatSessionDefaultConfigurationDto.TextStream},
            { "imageSize", chatSessionDefaultConfigurationDto.ImageSize ?? ""},
            { "imagesToGenerate", chatSessionDefaultConfigurationDto.ImagesToGenerate}
            
        };

        await AppwriteDatabase.UpdateDocument(AppwriteDatabaseId, ChatSessionDefaultConfigurationCollectionId,
            chatSessionDefaultConfigurationDto.Id, chatSessionConfigurationDocument);
    }

    public async Task<ChatSessionConfigurationDto?> GetChatSessionConfiguration(string chatSessionId)
    {
        var chatSessionConfigurationDocuments = await AppwriteDatabase.ListDocuments(AppwriteDatabaseId, 
            ChatSessionConfigurationCollectionId, 
            [
                Query.Equal("chatSession", chatSessionId)
            ]);

        if (chatSessionConfigurationDocuments.Documents.Count > 0) 
        {
            var chatSessionConfiguration = ConvertToChatSessionConfiguration(chatSessionConfigurationDocuments.Documents.ElementAt(0));
            return chatSessionConfiguration;
        }

        return null;
    }

    public async Task SaveChatSessionConfiguration(ChatSessionConfigurationDto chatSessionConfigurationDto, string chatSessionId)
    {
        var chatSessionConfigurationDocument = new Dictionary<string, object>()
        {
            { "chatbotName", chatSessionConfigurationDto.ChatbotName ?? ""},
            { "textGenerationChatbotModel", chatSessionConfigurationDto.TextGenerationChatbotModel ?? ""},
            { "imageGenerationChatbotModel", chatSessionConfigurationDto.ImageGenerationChatbotModel ?? ""},
            { "temperature", chatSessionConfigurationDto.Temperature },
            { "systemInstruction", chatSessionConfigurationDto.SystemInstruction ?? ""},
            { "textStream", chatSessionConfigurationDto.TextStream },
            { "imageSize", chatSessionConfigurationDto.ImageSize ?? ""},
            { "imagesToGenerate", chatSessionConfigurationDto.ImagesToGenerate},
            { "chatSession", chatSessionId}
        };

        await AppwriteDatabase.CreateDocument(AppwriteDatabaseId, ChatSessionConfigurationCollectionId,
            ID.Unique(), chatSessionConfigurationDocument);
    }

    public async Task EditChatSessionConfiguration(ChatSessionConfigurationDto chatSessionConfigurationDto, string chatSessionId)
    {
        if (string.IsNullOrEmpty(chatSessionConfigurationDto.Id))
        {
            throw new ArgumentException("The document ID must be provided.", nameof(chatSessionConfigurationDto.Id));
        }
        
        var chatSessionConfigurationDocument = new Dictionary<string, object>()
        {
            { "chatbotName", chatSessionConfigurationDto.ChatbotName ?? ""},
            { "textGenerationChatbotModel", chatSessionConfigurationDto.TextGenerationChatbotModel ?? ""},
            { "imageGenerationChatbotModel", chatSessionConfigurationDto.ImageGenerationChatbotModel ?? ""},
            { "temperature", chatSessionConfigurationDto.Temperature },
            { "systemInstruction", chatSessionConfigurationDto.SystemInstruction ?? ""},
            { "textStream", chatSessionConfigurationDto.TextStream },
            { "imageSize", chatSessionConfigurationDto.ImageSize ?? ""},
            { "imagesToGenerate", chatSessionConfigurationDto.ImagesToGenerate},
            { "chatSession", chatSessionId}
        };

        await AppwriteDatabase.UpdateDocument(AppwriteDatabaseId, ChatSessionConfigurationCollectionId,
            chatSessionConfigurationDto.Id, chatSessionConfigurationDocument);
    }

    public async Task DeleteChatSessionConfiguration(string id)
    {
        await AppwriteDatabase.DeleteDocument(AppwriteDatabaseId, ChatSessionConfigurationCollectionId, id);
    }

    private ChatbotDefaultConfigurationDto ConvertToChatbotDefaultConfiguration(
        Document chatbotDefaultConfigurationDocument)
    {
        var chatbotDefaultConfigurationDto = new ChatbotDefaultConfigurationDto()
        {
            Id = chatbotDefaultConfigurationDocument.Id,
            ChatbotName = chatbotDefaultConfigurationDocument.Data["chatbotName"].ToString(),
            TextGenerationChatbotModel = chatbotDefaultConfigurationDocument.Data["textGenerationChatbotModel"].ToString(),
            ImageGenerationChatbotModel = chatbotDefaultConfigurationDocument.Data["imageGenerationChatbotModel"].ToString(),
            ApiHost = chatbotDefaultConfigurationDocument.Data["apiHost"].ToString(),
            ApiKey = chatbotDefaultConfigurationDocument.Data["apiKey"].ToString(),
            Temperature = float.Parse(chatbotDefaultConfigurationDocument.Data["temperature"].ToString() ?? "0.0"),
            Selected = bool.Parse(chatbotDefaultConfigurationDocument.Data["selected"].ToString() ?? "false")
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
            TextStream = bool.Parse(chatSessionDefaultConfigurationDocument.Data["textStream"].ToString() ?? "false"),
            ImageSize = chatSessionDefaultConfigurationDocument.Data["imageSize"].ToString(),
            ImagesToGenerate = int.Parse(chatSessionDefaultConfigurationDocument.Data["imagesToGenerate"].ToString() ?? "1")
        };

        return chatSessionDefaultConfigurationDto;
    }

    private ChatSessionConfigurationDto ConvertToChatSessionConfiguration(Document chatSessionConfigurationDocument)
    {
        var chatSessionConfigurationDto = new ChatSessionConfigurationDto()
        {
            Id = chatSessionConfigurationDocument.Id,
            ChatbotName = chatSessionConfigurationDocument.Data["chatbotName"].ToString(),
            TextGenerationChatbotModel = chatSessionConfigurationDocument.Data["textGenerationChatbotModel"].ToString(),
            ImageGenerationChatbotModel = chatSessionConfigurationDocument.Data["imageGenerationChatbotModel"].ToString(),
            Temperature = float.Parse(chatSessionConfigurationDocument.Data["temperature"].ToString() ?? "0.0"),
            SystemInstruction = chatSessionConfigurationDocument.Data["systemInstruction"].ToString(),
            TextStream = bool.Parse(chatSessionConfigurationDocument.Data["textStream"].ToString() ?? "false"),
            ImageSize = chatSessionConfigurationDocument.Data["imageSize"].ToString(),
            ImagesToGenerate = int.Parse(chatSessionConfigurationDocument.Data["imagesToGenerate"].ToString() ?? "1")
        };

        return chatSessionConfigurationDto;
    }
}