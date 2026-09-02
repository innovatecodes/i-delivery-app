namespace IDelivery.Infrastructure.Messaging.Email;

public sealed class EmailOptions
{
    public bool EnableEmailSending { get; init; } 
    public bool UseSsl { get; init; }
    public int SmtpPort { get; init; }
    public string FromName { get; init; } = string.Empty;

    public string SmtpUser { get; init; } = string.Empty;
    public string SmtpPass { get; init; } = string.Empty;
    public string SmtpHost { get; init; } = string.Empty;
    public string FromEmail { get; init; } = string.Empty;

    public string ClientUrl { get; init; } = string.Empty;
}