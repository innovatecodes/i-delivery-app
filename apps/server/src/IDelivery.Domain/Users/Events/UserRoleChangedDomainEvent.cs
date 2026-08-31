using IDelivery.Domain.Common.DomainEvents;
using IDelivery.Domain.Roles;

namespace IDelivery.Domain.Users.Events;

/// <summary>
/// Evento disparado quando o role do usuário é alterado.
/// </summary>
public sealed class UserRoleChangedDomainEvent : DomainEvent
{
    public Guid UserId { get; }
    public string Email { get; }
    public Role NewRole { get; }
    public Guid? NewTenantId { get; }

    public UserRoleChangedDomainEvent(Guid userId, string email, Role newRole, Guid? newTenantId)
    {
        UserId = userId;
        Email = email;
        NewRole = newRole;
        NewTenantId = newTenantId;
    }
}