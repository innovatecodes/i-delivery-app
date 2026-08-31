using IDelivery.Domain.Common.DomainEvents;

namespace IDelivery.Domain.Users.Events;

/// <summary>
/// Evento disparado quando o perfil do usuário é atualizado.
/// </summary>
public sealed class UserProfileUpdatedDomainEvent : DomainEvent
{
    public Guid UserId { get; }
    public string FullName { get; }
    public string? PhoneNumber { get; }

    public UserProfileUpdatedDomainEvent(Guid userId, string fullName, string? phoneNumber)
    {
        UserId = userId;
        FullName = fullName;
        PhoneNumber = phoneNumber;
    }
}