using ProductsSales.Application.DTOs;
using ProductsSales.Application.Interfaces;
using BCrypt.Net;

namespace ProductsSales.Application.Services;

public class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;

    public AuthService(IUserRepository userRepository, IJwtService jwtService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByUsernameAsync(dto.Username, cancellationToken);
        if (user == null)
            return null;

        if (!VerifyPassword(dto.Password, user.PasswordHash))
            return null;

        var token = _jwtService.GenerateToken(user.Id, user.Username);

        return new LoginResponseDto(token, user.Id, user.Username);
    }

    private static bool VerifyPassword(string password, string passwordHash)
    {
        return password == passwordHash || BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}

