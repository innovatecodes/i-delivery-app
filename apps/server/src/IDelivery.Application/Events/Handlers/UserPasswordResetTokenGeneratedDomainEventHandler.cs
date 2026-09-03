using IDelivery.Application.Abstractions.Events;
using IDelivery.Application.Abstractions.Messaging;
using IDelivery.Application.Common.Models;
using IDelivery.Domain.Users.Events;
using Microsoft.Extensions.Logging;

namespace IDelivery.Application.Events.Handlers;

public sealed class UserPasswordResetTokenGeneratedDomainEventHandler : IDomainEventHandler<UserPasswordResetTokenGeneratedDomainEvent>
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<UserPasswordResetTokenGeneratedDomainEventHandler> _logger;

    public UserPasswordResetTokenGeneratedDomainEventHandler(
        INotificationService notificationService,
        ILogger<UserPasswordResetTokenGeneratedDomainEventHandler> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task Handle(UserPasswordResetTokenGeneratedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var payload = new UserPasswordResetPayload(domainEvent.ResetToken);

        try
        {
            await _notificationService.NotifyAsync<UserPasswordResetPayload>(
                recipient: domainEvent.Email,
                payload: payload,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Falha ao enviar e-mail de redefinição de senha para o usuário {UserId} ({Email}). O token está seguro no banco, mas o envio falhou",
                domainEvent.UserId,
                domainEvent.Email);
        }
    }
}
