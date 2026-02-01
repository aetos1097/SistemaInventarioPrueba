using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using ProductsSales.Application.DTOs;
using ProductsSales.Application.Services;

namespace ProductsSales.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly IValidator<LoginDto> _validator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(AuthService authService, IValidator<LoginDto> validator, ILogger<AuthController> logger)
    {
        _authService = authService;
        _validator = validator;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        var validationResult = await _validator.ValidateAsync(loginDto);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
        }

        var result = await _authService.LoginAsync(loginDto);
        
        if (result == null)
        {
            _logger.LogWarning("Failed login attempt for user: {Username}", loginDto.Username);
            return Unauthorized(new { message = "Usuario o contraseña incorrectos" });
        }

        _logger.LogInformation("User logged in: {Username}", loginDto.Username);
        return Ok(result);
    }
}

