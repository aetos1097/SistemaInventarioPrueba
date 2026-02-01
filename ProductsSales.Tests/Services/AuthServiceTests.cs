using FluentAssertions;
using Moq;
using ProductsSales.Application.DTOs;
using ProductsSales.Application.Interfaces;
using ProductsSales.Application.Services;
using ProductsSales.Domain.Entities;
using Xunit;

namespace ProductsSales.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly Mock<IJwtService> _mockJwtService;
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _mockUserRepo = new Mock<IUserRepository>();
        _mockJwtService = new Mock<IJwtService>();
        _service = new AuthService(_mockUserRepo.Object, _mockJwtService.Object);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ShouldReturnLoginResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var password = "password123";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
        var user = CreateUser(userId, "testuser", passwordHash);
        var loginDto = new LoginDto("testuser", password);

        _mockUserRepo.Setup(r => r.GetByUsernameAsync("testuser", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _mockJwtService.Setup(s => s.GenerateToken(userId, "testuser")).Returns("jwt-token");

        // Act
        var result = await _service.LoginAsync(loginDto);

        // Assert
        result.Should().NotBeNull();
        result!.Token.Should().Be("jwt-token");
        result.UserId.Should().Be(userId);
        result.Username.Should().Be("testuser");
    }

    [Fact]
    public async Task LoginAsync_InvalidPassword_ShouldReturnNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword");
        var user = CreateUser(userId, "testuser", passwordHash);
        var loginDto = new LoginDto("testuser", "wrongpassword");

        _mockUserRepo.Setup(r => r.GetByUsernameAsync("testuser", It.IsAny<CancellationToken>())).ReturnsAsync(user);

        // Act
        var result = await _service.LoginAsync(loginDto);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_UserNotFound_ShouldReturnNull()
    {
        // Arrange
        var loginDto = new LoginDto("nonexistent", "password");

        _mockUserRepo.Setup(r => r.GetByUsernameAsync("nonexistent", It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        // Act
        var result = await _service.LoginAsync(loginDto);

        // Assert
        result.Should().BeNull();
    }

    private static User CreateUser(Guid id, string username, string passwordHash)
    {
        return new User
        {
            Id = id,
            Username = username,
            PasswordHash = passwordHash
        };
    }
}
