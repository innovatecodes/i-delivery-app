using IDelivery.Domain.Common.DomainEvents;

namespace IDelivery.Domain.Users.Events
{
    /// <summary>/// Evento disparado quando o token de ativação do usuário é gerado e persistido./// </summary>
    public class UserActivationTokenGeneratedDomainEvent : DomainEvent
    {
        public Guid UserId { get; }
        public string Email { get; } 
        public string ActivationToken { get; } 

        public UserActivationTokenGeneratedDomainEvent(Guid userId, string email, string activationToken)
        {
            UserId = userId;
            Email = email;
            ActivationToken = activationToken;
        }
    }
}
