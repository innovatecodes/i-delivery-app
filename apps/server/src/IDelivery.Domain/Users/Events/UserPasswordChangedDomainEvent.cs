using IDelivery.Domain.Common.DomainEvents;

namespace IDelivery.Domain.Users.Events;

/// <summary>
/// Evento disparado quando a senha do usuário é alterada.
/// </summary>
public sealed class UserPasswordChangedDomainEvent : DomainEvent
{
    public Guid UserId { get; }
    public string Email { get; }

    public UserPasswordChangedDomainEvent(Guid userId, string email)
    {
        UserId = userId;
        Email = email;
    }
}