using IDelivery.Domain.Orders.Entities;
using IDelivery.Domain.Orders.Enums;

namespace IDelivery.Application.Abstractions.Persistence;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Order?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetByTenantIdAsync(Guid tenantId, OrderState? state = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetByCustomerIdAsync(Guid tenantId, Guid customerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetByDriverIdAsync(Guid tenantId, Guid driverId, OrderState? state = null, CancellationToken cancellationToken = default);
    Task<int> CountByTenantIdAsync(Guid tenantId, OrderState? state = null, CancellationToken cancellationToken = default);
    Task AddAsync(Order order, CancellationToken cancellationToken = default);
    Task UpdateAsync(Order order, CancellationToken cancellationToken = default);
}