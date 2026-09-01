using IDelivery.Domain.Delivery.Entities;

namespace IDelivery.Application.Abstractions.Persistence;

public interface IDeliverySettingsRepository
{
    Task<DeliverySettings?> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task AddAsync(DeliverySettings settings, CancellationToken cancellationToken = default);
    Task UpdateAsync(DeliverySettings settings, CancellationToken cancellationToken = default);
    Task DeleteAsync(DeliverySettings settings, CancellationToken cancellationToken = default);
}