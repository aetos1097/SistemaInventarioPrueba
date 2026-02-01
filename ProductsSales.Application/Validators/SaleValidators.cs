using FluentValidation;
using ProductsSales.Application.DTOs;

namespace ProductsSales.Application.Validators;

public class CreateSaleValidator : AbstractValidator<CreateSaleDto>
{
    public CreateSaleValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("La venta debe tener al menos un item");

        RuleForEach(x => x.Items).SetValidator(new CreateSaleItemValidator());
    }
}

public class CreateSaleItemValidator : AbstractValidator<CreateSaleItemDto>
{
    public CreateSaleItemValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Debe seleccionar un producto válido");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("La cantidad debe ser mayor a 0");
    }
}
