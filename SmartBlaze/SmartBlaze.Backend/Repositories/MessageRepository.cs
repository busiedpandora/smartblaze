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

          List<MessageDto> messages = new();

          foreach (var messageDocument in messageDocuments.Documents)
          {
               var userImageDocuments = await AppwriteDatabase.ListDocuments(AppwriteDatabaseId, UserImageCollectionId,
               [
                    Query.Equal("message", messageDocument.Id)
               ]);

               var message = ConvertToMessage(messageDocument, userImageDocuments.Documents);
               
               messages.Add(message);
          }

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

          var messageStoredDocument = await AppwriteDatabase.CreateDocument(AppwriteDatabaseId, MessageCollectionId, 
               ID.Unique(), messageDocument);

          if (messageDto.UserImageDtos is not null && messageDto.UserImageDtos.Count > 0)
          {
               foreach (var userImageDto in messageDto.UserImageDtos)
               {
                    var userImageDocument = new Dictionary<string, object>()
                    {
                         { "content", userImageDto.Content },
                         { "type", userImageDto.Type },
                         { "contentType", userImageDto.ContentType },
                         { "message", messageStoredDocument.Id }
                    };

                    await AppwriteDatabase.CreateDocument(AppwriteDatabaseId, UserImageCollectionId, 
                         ID.Unique(), userImageDocument);
               }
          }
     }

     private MessageDto ConvertToMessage(Document messageDocument, List<Document> userImageDocuments)
     {
          var messageDto = new MessageDto()
          {
               Content = messageDocument.Data["content"].ToString(),
               Role = messageDocument.Data["role"].ToString(),
               CreationDate = DateTime.Parse(messageDocument.Data["creationDate"].ToString() ?? "")
          };

          if (userImageDocuments.Count > 0)
          {
               List<UserImageDto> userImageDtos = new();
               
               foreach (var userImageDocument in userImageDocuments)
               {
                    var userImageDto = new UserImageDto()
                    {
                         Type = userImageDocument.Data["type"].ToString(),
                         Content = userImageDocument.Data["content"].ToString(),
                         ContentType = userImageDocument.Data["contentType"].ToString()
                    };
                    
                    userImageDtos.Add(userImageDto);
               }

               messageDto.UserImageDtos = userImageDtos;
          }

          return messageDto;
     }
}