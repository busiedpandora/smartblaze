using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SmartBlaze.Backend.Controllers;
using SmartBlaze.Backend.Dtos;
using SmartBlaze.Backend.Services;

namespace SmartBlaze.Backend.Tests.Controllers;

[TestSubject(typeof(UserController))]
public class UserControllerTest
{

    [Fact]
    public async void GetUser()
    {
        var userServiceMock = new Mock<IUserService>();
        var userController = new UserController(userServiceMock.Object);
        var user = new UserDto();
        user.Username = "username";
        user.Password = "password";

        userServiceMock.Setup(service => service.GetUserByUsername(user.Username))
            .ReturnsAsync(user);

        userServiceMock.Setup(service => service.VerifyPassword(user, user.Password, user.Password))
            .Returns(true);
        
        var result = await userController.GetUser(user);
        
        userServiceMock.Verify(service => service.VerifyPassword(user, user.Password, user.Password),
            Times.Once);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<UserDto>(okResult.Value);
        Assert.Equal("username", returnValue.Username);
        Assert.Equal("password", returnValue.Password);
    }
    
    [Fact]
    public async void RegisterNewUser()
    {
        var userServiceMock = new Mock<IUserService>();
        var userController = new UserController(userServiceMock.Object);
        var user = new UserDto();
        user.Username = "username";
        user.Password = "password";

        userServiceMock.Setup(service => service.GetUserByUsername(user.Username))
            .ReturnsAsync((UserDto?)null);

        userServiceMock.Setup(service => service.HashPassword(user, user.Password))
            .Returns("password+++");
        
        userServiceMock.Setup(service => service.AddNewUser(user))
            .ReturnsAsync(user);
        
        var result = await userController.RegisterNewUser(user);
        
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<UserDto>(okResult.Value);
        Assert.Equal("username", returnValue.Username);
        Assert.Equal("password+++", returnValue.Password);
    }
}