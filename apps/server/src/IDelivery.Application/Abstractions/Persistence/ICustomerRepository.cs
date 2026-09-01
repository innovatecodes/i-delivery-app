using IDelivery.Domain.Customers.Entities;

namespace IDelivery.Application.Abstractions.Persistence;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Customer?> GetByUserIdAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
    Task<Customer?> GetByEmailAsync(Guid tenantId, string email, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAsync(Guid tenantId, string email, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task AddAsync(Customer customer, CancellationToken cancellationToken = default);
    Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default);
    Task DeleteAsync(Customer customer, CancellationToken cancellationToken = default);
}
