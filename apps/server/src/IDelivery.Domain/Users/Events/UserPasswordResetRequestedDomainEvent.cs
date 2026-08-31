using IDelivery.Domain.Common.DomainEvents;

namespace IDelivery.Domain.Users.Events;

/// <summary>
/// Evento disparado quando um reset de senha é solicitado.
/// </summary>
public sealed class UserPasswordResetRequestedDomainEvent : DomainEvent
{
    public Guid UserId { get; }
    public string Email { get; }
    public string ResetToken { get; }

    public UserPasswordResetRequestedDomainEvent(Guid userId, string email, string resetToken)
    {
        UserId = userId;
        Email = email;
        ResetToken = resetToken;
    }
}