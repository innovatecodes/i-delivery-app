using IDelivery.Application.Abstractions.Events;
using IDelivery.Application.Abstractions.Messaging;
using IDelivery.Application.Common.Models;
using IDelivery.Domain.Users.Events;
using Microsoft.Extensions.Logging;

namespace IDelivery.Application.Events.Handlers
{
    public class UserActivationTokenGeneratedDomainEventHandler : IDomainEventHandler<UserActivationTokenGeneratedDomainEvent>
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<UserActivationTokenGeneratedDomainEventHandler> _logger;

        public UserActivationTokenGeneratedDomainEventHandler(INotificationService notificationService, ILogger<UserActivationTokenGeneratedDomainEventHandler> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task Handle(UserActivationTokenGeneratedDomainEvent domainEvent, CancellationToken cancellationToken = default)
        {
            var payload = new UserActivationPayload(domainEvent.ActivationToken);

            try
            {
                // Dispara a notificação
                await _notificationService.NotifyAsync<UserActivationPayload>(
                    recipient: domainEvent.Email,
                    payload: payload,
                    cancellationToken: cancellationToken
                );
            }
            catch (Exception ex)
            {
                // O token já foi salvo no banco pelo handler anterior. Se a notificação falhar, registramos o erro crítico sem quebrar o fluxo
                _logger.LogError(
                    ex,
                    "Falha ao processar a notificação de ativação para o usuário {UserId} ({Email}). O token está seguro no banco, mas o envio falhou",
                    domainEvent.UserId,
                    domainEvent.Email);
            }
        }
    }
}