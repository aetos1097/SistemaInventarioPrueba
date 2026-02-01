using ProductsSales.Application.DTOs;
using ProductsSales.Application.Interfaces;
using ProductsSales.Domain.Entities;

namespace ProductsSales.Application.Services;

public class SaleService
{
    private readonly ISaleRepository _saleRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SaleService(
        ISaleRepository saleRepository,
        IProductRepository productRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _saleRepository = saleRepository;
        _productRepository = productRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SaleDto> CreateSaleAsync(CreateSaleDto dto, CancellationToken cancellationToken = default)
    {
        var sale = new Sale
        {
            Id = Guid.NewGuid(),
            Date = DateTime.UtcNow,
            UserId = dto.UserId,
            Items = new List<SaleItem>()
        };

        // Valida stock y crea los ítems de la venta
        foreach (var itemDto in dto.Items)
        {
            var product = await _productRepository.GetByIdAsync(itemDto.ProductId, cancellationToken);
            if (product == null)
                throw new InvalidOperationException($"Producto con ID {itemDto.ProductId} no encontrado");

            if (!product.HasEnoughStock(itemDto.Quantity))
                throw new InvalidOperationException($"No hay suficiente stock para el producto {product.Name}. Stock disponible: {product.Stock}");

            var saleItem = new SaleItem
            {
                Id = Guid.NewGuid(),
                SaleId = sale.Id,
                ProductId = product.Id,
                Quantity = itemDto.Quantity,
                UnitPrice = product.Price,
            };
            saleItem.CalculateLineTotal();

            sale.Items.Add(saleItem);

            // Descuenta el stock del producto
            product.UpdateStock(itemDto.Quantity);
            await _productRepository.UpdateAsync(product, cancellationToken);
        }

        sale.CalculateTotal();

        var created = await _saleRepository.CreateAsync(sale, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await MapToDtoAsync(created, cancellationToken);
    }

    private async Task<SaleDto> MapToDtoAsync(Sale sale, CancellationToken cancellationToken)
    {
        var items = new List<SaleItemDto>();
        foreach (var item in sale.Items)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId, cancellationToken);
            items.Add(new SaleItemDto(
                item.Id,
                item.ProductId,
                product?.Name ?? "Desconocido",
                item.Quantity,
                item.UnitPrice,
                item.LineTotal
            ));
        }

        var user = sale.User ?? await _userRepository.GetByIdAsync(sale.UserId, cancellationToken);
        var username = user?.Username ?? "Desconocido";

        return new SaleDto(
            sale.Id,
            sale.Date,
            sale.Total,
            sale.UserId,
            username,
            items
        );
    }
}

