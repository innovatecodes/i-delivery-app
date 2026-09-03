using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using IDelivery.Application.Abstractions.Messaging;
using IDelivery.Application.Settings;

namespace IDelivery.Infrastructure.Messaging.Email;

public sealed class EmailService : IEmailService
{
    private readonly EmailOptions _emailOptions;
    private readonly ClientSettings _clientSettings;

    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IOptions<EmailOptions> emailOptions, 
        IOptions<ClientSettings> clientSettings,
        ILogger<EmailService> logger
        )
    {
        _emailOptions = emailOptions.Value;
        _clientSettings = clientSettings.Value;
        _logger = logger;
    }

    public async Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        if (!_emailOptions.EnableEmailSending)
        {
            _logger.LogInformation("[EMAIL] To: {To}", to);
            _logger.LogInformation("[EMAIL] Subject: {Subject}", subject);
            return;
        }

        using var message = new MailMessage
        {
            From = new MailAddress(_emailOptions.FromEmail, _emailOptions.FromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };
        message.To.Add(to);

        using var client = new SmtpClient(_emailOptions.SmtpHost, _emailOptions.SmtpPort)
        {
            EnableSsl = _emailOptions.UseSsl,
            Credentials = new NetworkCredential(_emailOptions.SmtpUser, _emailOptions.SmtpPass)
        };

        await client.SendMailAsync(message, cancellationToken);
    }
}