using SmartBlaze.Frontend.Dtos;
using SmartBlaze.Frontend.Models;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace SmartBlaze.Frontend.Services;

public class UserStateService(IHttpClientFactory httpClientFactory) : AbstractService(httpClientFactory)
{
    private UserDto? _userLogged;
    private RedirectionService _redirectionService;

    private string _authError = "";

    public UserStateService(IHttpClientFactory httpClientFactory, RedirectionService redirectionService) : this(httpClientFactory)
    {
        _redirectionService = redirectionService;
    }

    public UserDto? UserLogged => _userLogged;

    public string AuthError
    {
        get => _authError;
        set => _authError = value ?? throw new ArgumentNullException(nameof(value));
    }

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
            _authError = registerUserResponseContent;
            NotifyNavigateToPage("/register");
            NotifyRefreshView();
            return;
        }

        _authError = "";
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
            _authError = loginUserResponseContent;
            NotifyNavigateToPage("/login");
            NotifyRefreshView();
            return;
        }

        _authError = "";
        _userLogged = JsonSerializer.Deserialize<UserDto>(loginUserResponseContent);
        
        NotifyNavigateToPage("/");
        NotifyRefreshView();
    }

    public void Logout()
    {
        _userLogged = null;
        
        NotifyNavigateToPage("/welcome");
        NotifyRefreshView();
    }
}