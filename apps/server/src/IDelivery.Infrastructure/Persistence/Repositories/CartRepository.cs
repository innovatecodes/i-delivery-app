using Microsoft.EntityFrameworkCore;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.Domain.Carts.Entities;
using IDelivery.Infrastructure.Persistence.Context;

namespace IDelivery.Infrastructure.Persistence.Repositories;

public class CartRepository : ICartRepository
{
    private readonly ApplicationDbContext _context;

    public CartRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Cart?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Cart?> GetByUserIdAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.UserId == userId, cancellationToken);
    }

    public async Task<Cart?> GetBySessionIdAsync(Guid tenantId, string sessionId, CancellationToken cancellationToken = default)
    {
        return await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.SessionId == sessionId, cancellationToken);
    }

    public async Task AddAsync(Cart cart, CancellationToken cancellationToken = default)
    {
        await _context.Carts.AddAsync(cart, cancellationToken);
    }

    public async Task UpdateAsync(Cart cart, CancellationToken cancellationToken = default)
    {
        _context.Carts.Update(cart);
    }

    public async Task DeleteAsync(Cart cart, CancellationToken cancellationToken = default)
    {
        _context.Carts.Remove(cart);
    }
}
