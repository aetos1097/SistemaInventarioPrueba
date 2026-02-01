using ProductsSales.WinForms.Models;

namespace ProductsSales.WinForms.Services;

public class AuthService
{
    private readonly ApiClient _apiClient;
    private LoginResponse? _currentUser;

    public AuthService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public bool IsAuthenticated => _currentUser != null && !string.IsNullOrEmpty(_currentUser.Token);
    public string? CurrentUsername => _currentUser?.Username;
    public Guid? CurrentUserId => _currentUser?.UserId;

    public async Task<bool> LoginAsync(string username, string password)
    {
        try
        {
            var loginDto = new { Username = username, Password = password };
            var response = await _apiClient.PostAsync<LoginResponse>("api/Auth/login", loginDto);

            if (response != null && !string.IsNullOrEmpty(response.Token))
            {
                _currentUser = response;
                _apiClient.SetToken(response.Token);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al iniciar sesión: {ex.Message}", ex);
        }
    }

    public void Logout()
    {
        _currentUser = null;
        _apiClient.ClearToken();
    }
}
