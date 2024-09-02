using SmartBlaze.Backend.Dtos;

namespace SmartBlaze.Backend.Repositories;

public interface IUserRepository
{
    Task<UserDto?> GetUserByUsername(string username);
    Task<UserDto> SaveUser(UserDto userDto);
}