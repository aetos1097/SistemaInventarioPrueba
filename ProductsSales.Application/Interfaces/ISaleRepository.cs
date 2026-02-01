using ProductsSales.Domain.Entities;

namespace ProductsSales.Application.Interfaces;

public interface ISaleRepository
{
    Task<Sale> CreateAsync(Sale sale, CancellationToken cancellationToken = default);
    Task<List<Sale>> GetSalesByDateRangeAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}

