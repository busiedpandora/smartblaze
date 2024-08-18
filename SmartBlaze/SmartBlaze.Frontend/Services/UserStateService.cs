using SmartBlaze.Frontend.Dtos;
using SmartBlaze.Frontend.Models;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace SmartBlaze.Frontend.Services;

public class UserStateService(IHttpClientFactory httpClientFactory) : AbstractService(httpClientFactory)
{
    private UserDto? _userLogged;

    private SettingsService _settingsService;


    public UserStateService(IHttpClientFactory httpClientFactory, SettingsService settingsService) : this(httpClientFactory)
    {
        _settingsService = settingsService;
    }

    public UserDto? UserLogged => _userLogged;

    public async Task Register(UserRegister userRegister)
    {
        var userDto = new UserDto
        {
            Username = userRegister.Username,
            Password = userRegister.Password
        };

        var registerUserResponse = await HttpClient.PostAsJsonAsync("user/register", userDto);
        var registerUserResponseContent = await registerUserResponse.Content.ReadAsStringAsync();

        if (!registerUserResponse.IsSuccessStatusCode)
        {
            return;
        }

        _userLogged = JsonSerializer.Deserialize<UserDto>(registerUserResponseContent);
        
        NotifyNavigateToPage("/");
        NotifyRefreshView();
    }

    public async Task Login(UserLogin userLogin)
    {
        var userDto = new UserDto
        {
            Username = userLogin.Username,
            Password = userLogin.Password
        };
        
        var loginUserResponse = await HttpClient.PostAsJsonAsync("user/login", userDto);
        var loginUserResponseContent = await loginUserResponse.Content.ReadAsStringAsync();
        
        if (!loginUserResponse.IsSuccessStatusCode)
        {
            return;
        }
        
        _userLogged = JsonSerializer.Deserialize<UserDto>(loginUserResponseContent);
        
        NotifyNavigateToPage("/");
        NotifyRefreshView();
    }
}