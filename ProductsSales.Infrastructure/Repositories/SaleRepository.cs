using Microsoft.EntityFrameworkCore;
using ProductsSales.Application.Interfaces;
using ProductsSales.Domain.Entities;
using ProductsSales.Infrastructure.Data;

namespace ProductsSales.Infrastructure.Repositories;

public class SaleRepository : ISaleRepository
{
    private readonly ProductsSalesDbContext _context;

    public SaleRepository(ProductsSalesDbContext context)
    {
        _context = context;
    }

    public async Task<Sale> CreateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        await _context.Sales.AddAsync(sale, cancellationToken);
        return sale;
    }

    public async Task<List<Sale>> GetSalesByDateRangeAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        return await _context.Sales
            .Include(s => s.User)
            .Include(s => s.Items)
                .ThenInclude(i => i.Product)
            .Where(s => s.Date >= from && s.Date <= to)
            .OrderByDescending(s => s.Date)
            .ToListAsync(cancellationToken);
    }

    public async Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Sales
            .Include(s => s.User)
            .Include(s => s.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }
}

