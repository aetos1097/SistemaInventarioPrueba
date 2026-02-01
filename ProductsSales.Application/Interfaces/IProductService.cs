using ProductsSales.Application.DTOs;

namespace ProductsSales.Application.Interfaces;

/// <summary>
/// Servicio de productos (CRUD). Permite mockear en tests del controlador.
/// </summary>
public interface IProductService
{
    Task<ProductDto> CreateProductAsync(CreateProductDto dto, CancellationToken cancellationToken = default);
    Task<ProductDto?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<ProductDto>> GetAllProductsAsync(CancellationToken cancellationToken = default);
    Task<ProductDto?> UpdateProductAsync(Guid id, UpdateProductDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteProductAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Actualiza solo la URL de la imagen del producto (usado después de subir a Blob).</summary>
    Task<bool> UpdateImageUrlAsync(Guid id, string imageUrl, CancellationToken cancellationToken = default);
}
