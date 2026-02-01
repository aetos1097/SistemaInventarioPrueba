namespace ProductsSales.Application.DTOs;

public record CreateProductDto(
    string Name,
    decimal Price,
    int Stock,
    string? ImagePath
);

