using Microsoft.EntityFrameworkCore;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.Domain.Orders.Entities;
using IDelivery.Domain.Orders.Enums;
using IDelivery.Infrastructure.Persistence.Context;

namespace IDelivery.Infrastructure.Persistence.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _context;

    public OrderRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<Order?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetByTenantIdAsync(Guid tenantId, OrderState? state = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var query = _context.Orders.Where(o => o.TenantId == tenantId);

        if (state.HasValue)
            query = query.Where(o => o.State == state.Value);

        return await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetByCustomerIdAsync(Guid tenantId, Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Where(o => o.TenantId == tenantId && o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetByDriverIdAsync(Guid tenantId, Guid driverId, OrderState? state = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Orders.Where(o => o.TenantId == tenantId && o.DeliveryDriverId == driverId);

        if (state.HasValue)
            query = query.Where(o => o.State == state.Value);

        return await query
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountByTenantIdAsync(Guid tenantId, OrderState? state = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Orders.Where(o => o.TenantId == tenantId);

        if (state.HasValue)
            query = query.Where(o => o.State == state.Value);

        return await query.CountAsync(cancellationToken);
    }

    public async Task AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        await _context.Orders.AddAsync(order, cancellationToken);
    }

    public async Task UpdateAsync(Order order, CancellationToken cancellationToken = default)
    {
        _context.Orders.Update(order);
    }
}