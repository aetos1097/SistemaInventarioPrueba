using FluentValidation;
using ProductsSales.Application.DTOs;

namespace ProductsSales.Application.Validators;

public class LoginValidator : AbstractValidator<LoginDto>
{
    public LoginValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("El nombre de usuario es requerido");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es requerida");
    }
}
