namespace ProductsSales.Application.DTOs;

public record SaleDto(
    Guid Id,
    DateTime Date,
    decimal Total,
    Guid UserId,
    string Username,
    List<SaleItemDto> Items
);

