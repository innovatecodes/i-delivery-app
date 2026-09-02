using IDelivery.Domain.Common.DomainEvents;

namespace IDelivery.Domain.Users.Events;

/// <summary>
/// Evento disparado quando um reset de senha é solicitado.
/// O token original é gerado e enviado pela Application layer (não trafega pelo Domain).
/// </summary>
public sealed class UserPasswordResetRequestedDomainEvent : DomainEvent
{
    public Guid UserId { get; }
    public string Email { get; }

    public UserPasswordResetRequestedDomainEvent(Guid userId, string email)
    {
        UserId = userId;
        Email = email;
    }
}