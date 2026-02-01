namespace ProductsSales.Application.DTOs;

public record ProductDto(
    Guid Id,
    string Name,
    decimal Price,
    int Stock,
    string? ImagePath,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

