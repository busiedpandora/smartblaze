using Appwrite;
using Appwrite.Models;
using SmartBlaze.Backend.Dtos;

namespace SmartBlaze.Backend.Repositories;

public class UserRepository : AbstractRepository, IUserRepository
{
    public async Task<UserDto?> GetUserByUsername(string username)
    {
        var userDocuments = await AppwriteDatabase.ListDocuments(AppwriteDatabaseId, UserCollectionId,
            [
                Query.Equal("username", username)
            ]);

        if (userDocuments.Documents.Count > 0)
        {
            var user = ConvertToUser(userDocuments.Documents.ElementAt(0));

            return user;
        }

        return null;
    }

    public async Task<UserDto> SaveUser(UserDto userDto)
    {
        var userDocument = new Dictionary<string, object>
        {
            { "username", userDto.Username ?? ""},
            { "password", userDto.Password ?? ""}
        };

        var uD = await AppwriteDatabase.CreateDocument(AppwriteDatabaseId, UserCollectionId,
            ID.Unique(), userDocument);

        return ConvertToUser(uD);
    }

    private UserDto ConvertToUser(Document userDocument)
    {
        return new UserDto
        {
            Id = userDocument.Id,
            Username = userDocument.Data["username"].ToString() ?? "",
            Password = userDocument.Data["password"].ToString() ?? ""
        };
    }
}