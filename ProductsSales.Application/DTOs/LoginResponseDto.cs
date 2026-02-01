namespace ProductsSales.Application.DTOs;

public record LoginResponseDto(
    string Token,
    Guid UserId,
    string Username
);

