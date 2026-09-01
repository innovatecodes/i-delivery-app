using IDelivery.Domain.Carts.Entities;

namespace IDelivery.Application.Abstractions.Persistence;

public interface ICartRepository
{
    Task<Cart?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Cart?> GetByUserIdAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
    Task<Cart?> GetBySessionIdAsync(Guid tenantId, string sessionId, CancellationToken cancellationToken = default);
    Task AddAsync(Cart cart, CancellationToken cancellationToken = default);
    Task UpdateAsync(Cart cart, CancellationToken cancellationToken = default);
    Task DeleteAsync(Cart cart, CancellationToken cancellationToken = default);
}
