namespace ProductsSales.Application.DTOs;

public record CreateSaleDto(
    Guid UserId,
    List<CreateSaleItemDto> Items
);

