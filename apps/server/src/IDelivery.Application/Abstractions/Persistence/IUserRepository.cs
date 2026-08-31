using IDelivery.Domain.Roles;
using IDelivery.Domain.Users.Entities;
using IDelivery.Domain.Users.Enums;

namespace IDelivery.Application.Abstractions.Persistence;

/// <summary>
/// Repositório para operações de User.
/// </summary>
public interface IUserRepository : IRepository<User>
{
    /// <summary>
    /// Busca usuário por email.
    /// </summary>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se existe usuário com o email.
    /// </summary>
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca usuários por tenant.
    /// </summary>
    Task<IReadOnlyList<User>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca usuários por role.
    /// </summary>
    Task<IReadOnlyList<User>> GetByRoleAsync(Role role, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca usuários por status.
    /// </summary>
    Task<IReadOnlyList<User>> GetByStatusAsync(UserStatus status, CancellationToken cancellationToken = default);
}