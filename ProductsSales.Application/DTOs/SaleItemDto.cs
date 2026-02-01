namespace ProductsSales.Application.DTOs;

public record SaleItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal
);

