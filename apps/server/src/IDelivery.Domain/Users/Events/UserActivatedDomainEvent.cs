using IDelivery.Domain.Common.DomainEvents;

namespace IDelivery.Domain.Users.Events;

/// <summary>
/// Evento disparado quando um usuário ativa sua conta.
/// </summary>
public sealed class UserActivatedDomainEvent : DomainEvent
{
    public Guid UserId { get; }
    public string Email { get; }

    public UserActivatedDomainEvent(Guid userId, string email)
    {
        UserId = userId;
        Email = email;
    }
}