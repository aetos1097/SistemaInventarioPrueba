namespace ProductsSales.Application.DTOs;

public record CreateSaleItemDto(
    Guid ProductId,
    int Quantity
);

