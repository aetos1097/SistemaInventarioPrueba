using Microsoft.EntityFrameworkCore;
using ProductsSales.Application.Interfaces;
using ProductsSales.Domain.Entities;
using ProductsSales.Infrastructure.Data;

namespace ProductsSales.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ProductsSalesDbContext _context;

    public UserRepository(ProductsSalesDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users.FindAsync([id], cancellationToken);
    }
}

