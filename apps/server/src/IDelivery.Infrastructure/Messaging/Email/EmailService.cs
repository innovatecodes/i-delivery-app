using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using IDelivery.Application.Abstractions.Messaging;

namespace IDelivery.Infrastructure.Messaging.Email;

public sealed class EmailService : IEmailService
{
    private readonly EmailOptions _options;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailOptions> options, ILogger<EmailService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        if (!_options.EnableEmailSending)
        {
            // Log email instead of sending in development (no SMTP configured)
            _logger.LogInformation("[EMAIL] To: {To}", to);
            _logger.LogInformation("[EMAIL] Subject: {Subject}", subject);
            _logger.LogInformation("[EMAIL] Body: {Body}", body);
            return;
        }

        using var message = new MailMessage
        {
            From = new MailAddress(_options.FromEmail, _options.FromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };
        message.To.Add(to);

        using var client = new SmtpClient(_options.SmtpHost, _options.SmtpPort)
        {
            EnableSsl = _options.UseSsl,
            Credentials = new NetworkCredential(_options.SmtpUser, _options.SmtpPass)
        };

        await client.SendMailAsync(message, cancellationToken);
    }

    public async Task SendActivationEmailAsync(string to, string activationLink, CancellationToken cancellationToken = default)
    {
        var subject = "Ative sua conta - iDelivery";
        var body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #2563eb; color: white; text-decoration: none; border-radius: 6px; }}
        .footer {{ margin-top: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <h2>Bem-vindo ao iDelivery!</h2>
        <p>Obrigado por se cadastrar. Para ativar sua conta, clique no botão abaixo:</p>
        <p style='text-align: center; margin: 30px 0;'>
            <a href='{activationLink}' class='button'>Ativar Conta</a>
        </p>
        <p>Ou copie e cole este link no navegador:</p>
        <p style='word-break: break-all; color: #2563eb;'>{activationLink}</p>
        <p>Este link expira em 24 horas.</p>
        <div class='footer'>
            <p>Se você não criou esta conta, por favor ignore este e-mail.</p>
            <p>&copy; {DateTime.UtcNow.Year} iDelivery. Todos os direitos reservados.</p>
        </div>
    </div>
</body>
</html>";

        await SendAsync(to, subject, body, cancellationToken);
    }
}