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
               var mediaDocuments = await AppwriteDatabase.ListDocuments(AppwriteDatabaseId, MediaCollectionId,
               [
                    Query.Equal("message", messageDocument.Id)
               ]);

               var message = ConvertToMessage(messageDocument, mediaDocuments.Documents);
               
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
               {"text", messageDto.Text},
               {"role", messageDto.Role},
               {"creationDate", messageDto.CreationDate},
               {"chatSession", chatSessionDocument.Id}
          };

          var messageStoredDocument = await AppwriteDatabase.CreateDocument(AppwriteDatabaseId, MessageCollectionId, 
               ID.Unique(), messageDocument);

          if (messageDto.MediaDtos is not null && messageDto.MediaDtos.Count > 0)
          {
               foreach (var mediaDto in messageDto.MediaDtos)
               {
                    var mediaDocument = new Dictionary<string, object>()
                    {
                         { "data", mediaDto.Data },
                         { "contentType", mediaDto.ContentType },
                         { "message", messageStoredDocument.Id }
                    };

                    await AppwriteDatabase.CreateDocument(AppwriteDatabaseId, MediaCollectionId, 
                         ID.Unique(), mediaDocument);
               }
          }
     }

     private MessageDto ConvertToMessage(Document messageDocument, List<Document> mediaDocuments)
     {
          var messageDto = new MessageDto()
          {
               Text = messageDocument.Data["text"].ToString(),
               Role = messageDocument.Data["role"].ToString(),
               CreationDate = DateTime.Parse(messageDocument.Data["creationDate"].ToString() ?? "")
          };

          if (mediaDocuments.Count > 0)
          {
               List<MediaDto> mediaDtos = new();
               
               foreach (var mediaDocument in mediaDocuments)
               {
                    var mediaDto = new MediaDto()
                    {
                         Data = mediaDocument.Data["data"].ToString(),
                         ContentType = mediaDocument.Data["contentType"].ToString()
                    };
                    
                    mediaDtos.Add(mediaDto);
               }

               messageDto.MediaDtos = mediaDtos;
          }

          return messageDto;
     }
}