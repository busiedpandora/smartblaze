using Appwrite;
using Appwrite.Models;
using SmartBlaze.Backend.Dtos;

namespace SmartBlaze.Backend.Repositories;

public class MessageRepository : AbstractRepository
{
     public async Task<List<MessageDto>> GetMessagesFromChatSession(string chatSessionId)
     {
          var messageDocuments = await AppwriteDatabase.ListDocuments(AppwriteDatabaseId, MessageCollectionId, 
          [
               Query.Equal("chatSession", chatSessionId)
          ]);

          var messages = messageDocuments.Documents
               .Select(ConvertToMessage)
               .ToList();

          return messages;
     }
     
     public async Task SaveMessage(MessageDto messageDto, string chatSessionId)
     {
          var chatSessionDocument = await AppwriteDatabase.GetDocument(AppwriteDatabaseId, ChatSessionCollectionId, 
               chatSessionId);
          
          var messageDocument = new Dictionary<string, object>()
          {
               {"content", messageDto.Content},
               {"role", messageDto.Role},
               {"creationDate", messageDto.CreationDate},
               {"chatSession", chatSessionDocument.Id}
          };

          await AppwriteDatabase.CreateDocument(AppwriteDatabaseId, MessageCollectionId, 
               ID.Unique(), messageDocument);
     }

     private MessageDto ConvertToMessage(Document messageDocument)
     {
          var messageDto = new MessageDto()
          {
               Content = messageDocument.Data["content"].ToString(),
               Role = messageDocument.Data["role"].ToString(),
               CreationDate = DateTime.Parse(messageDocument.Data["creationDate"].ToString() ?? "")
          };

          return messageDto;
     }
}