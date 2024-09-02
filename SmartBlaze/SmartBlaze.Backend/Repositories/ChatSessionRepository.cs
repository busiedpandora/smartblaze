using System.Globalization;
using Appwrite;
using Appwrite.Models;
using SmartBlaze.Backend.Dtos;

namespace SmartBlaze.Backend.Repositories;

public class ChatSessionRepository : AbstractRepository, IChatSessionRepository
{ 
    public async Task<List<ChatSessionDto>> GetAllChatSessions(string userId)
    {
        var chatSessionDocuments = await AppwriteDatabase.ListDocuments(
                AppwriteDatabaseId, 
                ChatSessionCollectionId,
                [
                    Query.Equal("user", userId),
                    Query.OrderDesc("creationDate")
                ]
            );

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
    
    public async Task<ChatSessionDto> SaveChatSession(ChatSessionDto chatSessionDto, string userId)
    {
        if (chatSessionDto.Title?.Length > 30)
        {
            chatSessionDto.Title = chatSessionDto.Title.Substring(0, 30);
        }
        
        var chatSessionDocument = new Dictionary<string, object>()
        {
            {"title", chatSessionDto.Title ?? ""},
            {"creationDate", chatSessionDto.CreationDate ?? DateTime.MinValue},
            {"user", userId}
        };
        
        var csd = await AppwriteDatabase.CreateDocument(AppwriteDatabaseId, ChatSessionCollectionId, 
            ID.Unique(), chatSessionDocument);

        return ConvertToChatSession(csd);
    }

    public async Task EditChatSession(ChatSessionDto chatSessionDto)
    {
        if (string.IsNullOrEmpty(chatSessionDto.Id))
        {
            throw new ArgumentException("The document ID must be provided.", nameof(chatSessionDto.Id));
        }
        
        var chatSessionDocument = new Dictionary<string, object>()
        {
            {"title", chatSessionDto.Title ?? ""},
            {"creationDate", chatSessionDto.CreationDate ?? DateTime.MinValue},
        };
        
        var csd = await AppwriteDatabase.UpdateDocument(AppwriteDatabaseId, ChatSessionCollectionId, 
            chatSessionDto.Id, chatSessionDocument);
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