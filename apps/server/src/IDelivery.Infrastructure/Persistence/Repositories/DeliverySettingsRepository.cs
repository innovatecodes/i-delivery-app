using Microsoft.EntityFrameworkCore;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.Domain.Delivery.Entities;
using IDelivery.Infrastructure.Persistence.Context;

namespace IDelivery.Infrastructure.Persistence.Repositories;

public class DeliverySettingsRepository : IDeliverySettingsRepository
{
    private readonly ApplicationDbContext _context;

    public DeliverySettingsRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DeliverySettings?> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.DeliverySettings
            .FirstOrDefaultAsync(ds => ds.TenantId == tenantId, cancellationToken);
    }

    public async Task AddAsync(DeliverySettings settings, CancellationToken cancellationToken = default)
    {
        await _context.DeliverySettings.AddAsync(settings, cancellationToken);
    }

    public async Task UpdateAsync(DeliverySettings settings, CancellationToken cancellationToken = default)
    {
        _context.DeliverySettings.Update(settings);
    }

    public async Task DeleteAsync(DeliverySettings settings, CancellationToken cancellationToken = default)
    {
        _context.DeliverySettings.Remove(settings);
    }
}