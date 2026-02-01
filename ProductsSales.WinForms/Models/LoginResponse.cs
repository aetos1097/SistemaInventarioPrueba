namespace ProductsSales.WinForms.Models;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
}
