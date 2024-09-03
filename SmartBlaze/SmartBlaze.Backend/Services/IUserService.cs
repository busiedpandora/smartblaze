using SmartBlaze.Backend.Dtos;

namespace SmartBlaze.Backend.Services;

public interface IUserService
{
    Task<UserDto?> GetUserByUsername(string username);
    Task<UserDto> AddNewUser(UserDto userDto);
    string HashPassword(UserDto user, string password);
    bool VerifyPassword(UserDto user, string hashedPassword, string providedPassword);
}