using Microsoft.AspNetCore.Mvc;
using SmartBlaze.Backend.Dtos;
using SmartBlaze.Backend.Services;

namespace SmartBlaze.Backend.Controllers;

[ApiController]
[Route("user")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;


    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> GetUser([FromBody] UserDto userDto)
    {
        if (userDto.Username is null or "")
        {
            return BadRequest("The username of the user must be specified");
        }

        if (userDto.Password is null or "")
        {
            return BadRequest("The password for the user must be specified");
        }

        try
        {
            var user = await _userService.GetUserByUsername(userDto.Username);
            
            if (user is null)
            {
                return Conflict($"A user with the username {userDto.Username} does not exist.");
            }
            
            if (!_userService.VerifyPassword(user, user.Password ?? "", userDto.Password))
            {
                return Conflict("Incorrect password");
            }

            return Ok(user);
        }
        catch (Exception e)
        {
            return StatusCode(500, $"Request Error: {e.Message}");
        }
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> RegisterNewUser([FromBody] UserDto userDto)
    {
        if (userDto.Username is null or "")
        {
            return BadRequest("The username of the user must be specified");
        }

        if (userDto.Password is null or "")
        {
            return BadRequest("The password for the user must be specified");
        }

        try
        {
            var user = await _userService.GetUserByUsername(userDto.Username);

            if (user is not null)
            {
                return Conflict($"A user with the username {userDto.Username} already exists.");
            }

            userDto.Password = _userService.HashPassword(userDto, userDto.Password);

            user = await _userService.AddNewUser(userDto);

            return Ok(user);
        }
        catch (Exception e)
        {
            return StatusCode(500, $"Request Error: {e.Message}");
        }
    }
}