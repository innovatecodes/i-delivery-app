using IDelivery.Domain.Common.DomainEvents;

namespace IDelivery.Domain.Users.Events;

/// <summary>
/// Evento disparado quando um usuário é excluído (soft delete).
/// </summary>
public sealed class UserDeletedDomainEvent : DomainEvent
{
    public Guid UserId { get; }
    public string Email { get; }

    public UserDeletedDomainEvent(Guid userId, string email)
    {
        UserId = userId;
        Email = email;
    }
}