using JetBrains.Annotations;
using Microsoft.AspNetCore.Identity;
using Moq;
using SmartBlaze.Backend.Dtos;
using SmartBlaze.Backend.Repositories;
using SmartBlaze.Backend.Services;

namespace SmartBlaze.Backend.Tests.Services;

[TestSubject(typeof(UserService))]
public class UserServiceTest
{

    [Fact]
    public async void GetUserByUsername()
    {
        var userRepositoryMock = new Mock<IUserRepository>();
        var passwordHasherMock = new Mock<IPasswordHasher<UserDto>>();
        var userService = new UserService(userRepositoryMock.Object, passwordHasherMock.Object);
        var user = new UserDto();
        user.Username = "username";
        user.Password = "password";

        userRepositoryMock.Setup(repo => repo.GetUserByUsername(user.Username))
            .ReturnsAsync(user);

        var result = await userService.GetUserByUsername(user.Username);
        
        userRepositoryMock.Verify(repo => repo.GetUserByUsername(user.Username), Times.Once);
        Assert.Equal(user, result);
    }

    [Fact]
    public async void AddNewUser()
    {
        var userRepositoryMock = new Mock<IUserRepository>();
        var passwordHasherMock = new Mock<IPasswordHasher<UserDto>>();
        var userService = new UserService(userRepositoryMock.Object, passwordHasherMock.Object);
        var user = new UserDto();
        user.Username = "username";
        user.Password = "password";

        userRepositoryMock.Setup(repo => repo.SaveUser(user))
            .ReturnsAsync(user);

        var result = await userService.AddNewUser(user);
        
        userRepositoryMock.Verify(repo => repo.SaveUser(user), Times.Once);
        Assert.Equal(user, result);
    }

    [Fact]
    public void HashPassword()
    {
        var userRepositoryMock = new Mock<IUserRepository>();
        var passwordHasherMock = new Mock<IPasswordHasher<UserDto>>();
        var userService = new UserService(userRepositoryMock.Object, passwordHasherMock.Object);
        var user = new UserDto();
        user.Username = "username";
        user.Password = "password";

        passwordHasherMock.Setup(hasher => hasher.HashPassword(user, user.Password))
            .Returns("password+++");

        var result = userService.HashPassword(user, user.Password);
        
        passwordHasherMock.Verify(hasher => hasher.HashPassword(user, user.Password), Times.Once);
        Assert.Equal("password+++", result);
    }

    [Fact]
    public void VerifyPassword()
    {
        var userRepositoryMock = new Mock<IUserRepository>();
        var passwordHasherMock = new Mock<IPasswordHasher<UserDto>>();
        var userService = new UserService(userRepositoryMock.Object, passwordHasherMock.Object);
        var user = new UserDto();
        user.Username = "username";
        user.Password = "password";

        passwordHasherMock.Setup(hasher => hasher.VerifyHashedPassword(user, "password+++", 
            user.Password))
            .Returns(PasswordVerificationResult.Success);

        var result = userService.VerifyPassword(user, "password+++", user.Password);
        
        passwordHasherMock.Verify(hasher => hasher.VerifyHashedPassword(user, "password+++", 
            user.Password), Times.Once);
        Assert.True(result);
        
        passwordHasherMock.Setup(hasher => hasher.VerifyHashedPassword(user, "password+++", 
                user.Password))
            .Returns(PasswordVerificationResult.Failed);
        
        result = userService.VerifyPassword(user, "password+++", user.Password);
        Assert.False(result);
    }
}