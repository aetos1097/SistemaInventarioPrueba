namespace ProductsSales.Application.DTOs;

public record UpdateProductDto(
    string Name,
    decimal Price,
    int Stock,
    string? ImagePath
);

