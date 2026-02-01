namespace ProductsSales.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(Guid userId, string username);
}

