using System.Globalization;
using Appwrite;
using Appwrite.Models;
using SmartBlaze.Backend.Dtos;

namespace SmartBlaze.Backend.Repositories;

public class ChatSessionRepository : AbstractRepository
{ 
    public async Task<List<ChatSessionDto>> GetAllChatSessions()
    {
        var chatSessionDocuments = await AppwriteDatabase.ListDocuments(AppwriteDatabaseId, ChatSessionCollectionId);

        var chatSessionsDto = chatSessionDocuments.Documents
            .Select(ConvertToChatSession)
            .ToList();

        return chatSessionsDto;
    }

    public async Task<ChatSessionDto> GetChatSessionById(string id)
    {
        var chatSessionDocument = await AppwriteDatabase.GetDocument(AppwriteDatabaseId, ChatSessionCollectionId, id);

        var chatSessionDto = ConvertToChatSession(chatSessionDocument);

        return chatSessionDto;
    }
    
    public async Task<ChatSessionDto> SaveChatSession(ChatSessionDto chatSessionDto)
    {
        var chatSessionDocument = new Dictionary<string, object>()
        {
            {"title", chatSessionDto.Title ?? ""},
            {"creationDate", chatSessionDto.CreationDate ?? DateTime.MinValue},
        };
        
        var csd = await AppwriteDatabase.CreateDocument(AppwriteDatabaseId, ChatSessionCollectionId, 
            ID.Unique(), chatSessionDocument);

        return ConvertToChatSession(csd);
    }

    public async Task<ChatSessionDto> EditChatSession(ChatSessionDto chatSessionDto)
    {
        var chatSessionDocument = new Dictionary<string, object>()
        {
            {"title", chatSessionDto.Title ?? ""},
            {"creationDate", chatSessionDto.CreationDate ?? DateTime.MinValue},
        };
        
        var csd = await AppwriteDatabase.UpdateDocument(AppwriteDatabaseId, ChatSessionCollectionId, 
            ID.Unique(), chatSessionDocument);

        return ConvertToChatSession(csd);
    }
    
    private ChatSessionDto ConvertToChatSession(Document chatSessionDocument)
    {
        var chatSessionDto = new ChatSessionDto()
        {
            Id = chatSessionDocument.Id,
            Title = chatSessionDocument.Data["title"].ToString(),
            CreationDate = DateTime.Parse(chatSessionDocument.Data["creationDate"].ToString() 
                                          ?? DateTime.MinValue.ToString(CultureInfo.InvariantCulture))
        };

        return chatSessionDto;
    }
}