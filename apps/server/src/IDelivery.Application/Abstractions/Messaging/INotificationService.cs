
namespace IDelivery.Application.Abstractions.Messaging
{
    public interface INotificationService
    {
        /// <summary>
        /// Envia uma notificação genérica para um destinatário (independente de ser E-mail, SMS ou WhatsApp).
        /// </summary>
        /// <param name="recipient">O endereço de destino (E-mail, Telefone, ID de Push, etc.)</param>
        /// <param name="payload">Um objeto contendo os dados necessários para a notificação.</param>
        Task NotifyAsync<TPayload>(
            string recipient,
            TPayload payload,
            CancellationToken cancellationToken) where TPayload : class;
    }
}
