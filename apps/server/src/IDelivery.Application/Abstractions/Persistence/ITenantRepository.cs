using IDelivery.Domain.Tenants.Entities;
using IDelivery.Domain.Tenants.Enums;

namespace IDelivery.Application.Abstractions.Persistence;

public interface ITenantRepository : IRepository<Tenant>
{
    Task<Tenant?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Tenant>> GetAllAsync(
        int page,
        int pageSize,
        string? search = null,
        TenantStatus? status = null,
        CancellationToken cancellationToken = default);
    Task<int> CountAsync(
        string? search = null,
        TenantStatus? status = null,
        CancellationToken cancellationToken = default);
}