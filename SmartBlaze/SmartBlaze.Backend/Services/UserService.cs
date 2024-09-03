using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using SmartBlaze.Backend.Dtos;
using SmartBlaze.Backend.Repositories;

namespace SmartBlaze.Backend.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher<UserDto> _passwordHasher;

    public UserService(IUserRepository userRepository, IPasswordHasher<UserDto> passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<UserDto?> GetUserByUsername(string username)
    {
        return await _userRepository.GetUserByUsername(username);
    }

    public async Task<UserDto> AddNewUser(UserDto userDto)
    {
        return await _userRepository.SaveUser(userDto);
    }

    public string HashPassword(UserDto user, string password)
    {
        return _passwordHasher.HashPassword(user, password);
    }

    public bool VerifyPassword(UserDto user, string hashedPassword, string providedPassword)
    {
        var result = _passwordHasher.VerifyHashedPassword(user, hashedPassword, providedPassword);

        return result == PasswordVerificationResult.Success;
    }
}