using ProductsSales.Application.Interfaces;
using ProductsSales.Infrastructure.Data;

namespace ProductsSales.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ProductsSalesDbContext _context;

    public UnitOfWork(ProductsSalesDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}

