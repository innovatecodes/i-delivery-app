using IDelivery.Application.Abstractions.Messaging;
using IDelivery.Application.Common.Models;
using IDelivery.Application.Settings;
using IDelivery.Infrastructure.Messaging.Templates;
using Microsoft.Extensions.Options;

namespace IDelivery.Infrastructure.Messaging.Common;

public class NotificationService(
    IEmailService emailService,
    IClientUrlGenerator urlGenerator,
    IOptions<ClientSettings> clientSettings
    ) : INotificationService
{
    private readonly IEmailService _emailService = emailService;
    private readonly IClientUrlGenerator _urlGenerator = urlGenerator;
    private readonly ClientSettings _clientSettings = clientSettings.Value;

    public async Task NotifyAsync<TPayload>(string recipient, TPayload payload, CancellationToken cancellationToken) where TPayload : class
    {
        switch (payload)
        {
            case UserActivationPayload activation:
                await SendActivationEmailAsync(recipient, activation.Token, cancellationToken);
                break;

            case UserPasswordResetPayload reset:
                await SendPasswordResetEmailAsync(recipient, reset.Token, cancellationToken);
                break;

            default:
                throw new NotSupportedException($"Payload não suportado: {typeof(TPayload).Name}");
        }
    }

    private async Task SendActivationEmailAsync(string recipient, string token, CancellationToken ct)
    {
        var activationUrl = _urlGenerator.Generate(_clientSettings.Routes.Activate, new() { ["token"] = token });
        var body = EmailTemplate.Generate(
            "Bem-vindo ao iDelivery",
            "Obrigado por se cadastrar. Para ativar sua conta, clique no botão abaixo:",
            activationUrl,
            "Ativar Conta",
            "Se você não criou esta conta, por favor ignore este e-mail.");

        await _emailService.SendAsync(recipient, "Ativação", body, ct);
    }

    private async Task SendPasswordResetEmailAsync(string recipient, string token, CancellationToken ct)
    {
        var resetUrl = _urlGenerator.Generate(_clientSettings.Routes.Reset, new() { ["token"] = token });
        var body = EmailTemplate.Generate(
            "Redefinição de senha",
            "Você solicitou a redefinição de sua senha. Clique no botão abaixo para criar uma nova senha:",
            resetUrl,
            "Redefinir Senha",
            "Se você não solicitou esta redefinição, por favor ignore este e-mail.");

        await _emailService.SendAsync(recipient, "Redefinição de senha - iDelivery", body, ct);
    }
}
