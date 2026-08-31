using IDelivery.Domain.Common.DomainEvents;

namespace IDelivery.Domain.Users.Events;

/// <summary>
/// Evento disparado quando um usuário é desativado.
/// </summary>
public sealed class UserDeactivatedDomainEvent : DomainEvent
{
    public Guid UserId { get; }
    public string Email { get; }

    public UserDeactivatedDomainEvent(Guid userId, string email)
    {
        UserId = userId;
        Email = email;
    }
}