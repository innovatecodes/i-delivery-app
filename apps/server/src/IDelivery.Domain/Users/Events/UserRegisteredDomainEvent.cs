using IDelivery.Domain.Common.DomainEvents;
using IDelivery.Domain.Roles;

namespace IDelivery.Domain.Users.Events;

/// <summary>
/// Evento disparado quando um novo usuário é registrado.
/// </summary>
public sealed class UserRegisteredDomainEvent : DomainEvent
{
    public Guid UserId { get; }
    public string Email { get; }
    public string FullName { get; }
    public Role Role { get; }

    public UserRegisteredDomainEvent(Guid userId, string email, string fullName, Role role)
    {
        UserId = userId;
        Email = email;
        FullName = fullName;
        Role = role;
    }
}