using ProductsSales.Application.DTOs;
using ProductsSales.Application.Interfaces;

namespace ProductsSales.Application.Services;

public class ReportService
{
    private readonly ISaleRepository _saleRepository;
    private readonly IProductRepository _productRepository;

    public ReportService(ISaleRepository saleRepository, IProductRepository productRepository)
    {
        _saleRepository = saleRepository;
        _productRepository = productRepository;
    }

    public async Task<SalesReportDto> GetSalesReportAsync(DateTime from, DateTime to, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var allSales = await _saleRepository.GetSalesByDateRangeAsync(from, to, cancellationToken);
        
        // Total vendido en el rango de fechas
        decimal totalSold = 0;
        foreach (var sale in allSales)
        {
            totalSold += sale.Total;
        }
        
        var totalRecords = allSales.Count;
        var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
        
        // Paginación
        var paginatedSales = allSales
            .OrderByDescending(s => s.Date)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        
        var saleDtos = new List<SaleDto>();
        foreach (var sale in paginatedSales)
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

            saleDtos.Add(new SaleDto(
                sale.Id,
                sale.Date,
                sale.Total,
                sale.UserId,
                sale.User?.Username ?? "Desconocido",
                items
            ));
        }

        return new SalesReportDto(saleDtos, totalSold, page, pageSize, totalRecords, totalPages);
    }
}

