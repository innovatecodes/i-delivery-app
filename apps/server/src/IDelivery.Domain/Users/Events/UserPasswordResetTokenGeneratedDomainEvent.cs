using IDelivery.Domain.Common.DomainEvents;

namespace IDelivery.Domain.Users.Events;

/// <summary>
/// Evento disparado quando o token de reset de senha do usuário é gerado e persistido.
/// </summary>
public sealed class UserPasswordResetTokenGeneratedDomainEvent : DomainEvent
{
    public Guid UserId { get; }
    public string Email { get; }
    public string ResetToken { get; }

    public UserPasswordResetTokenGeneratedDomainEvent(Guid userId, string email, string resetToken)
    {
        UserId = userId;
        Email = email;
        ResetToken = resetToken;
    }
}
