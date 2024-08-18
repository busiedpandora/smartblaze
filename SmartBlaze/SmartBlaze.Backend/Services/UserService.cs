using System.Security.Cryptography;
using SmartBlaze.Backend.Dtos;
using SmartBlaze.Backend.Repositories;

namespace SmartBlaze.Backend.Services;

public class UserService
{
    private UserRepository _userRepository;


    public UserService(UserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto?> GetUserByUsername(string username)
    {
        return await _userRepository.GetUserByUsername(username);
    }

    public async Task<UserDto> AddNewUser(UserDto userDto)
    {
        return await _userRepository.SaveUser(userDto);
    }
}