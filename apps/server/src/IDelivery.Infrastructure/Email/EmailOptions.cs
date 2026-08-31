namespace IDelivery.Infrastructure.Email;

public sealed class EmailOptions
{
    public string SmtpHost { get; init; } = "localhost";
    public int SmtpPort { get; init; } = 587;
    public string SmtpUser { get; init; } = string.Empty;
    public string SmtpPass { get; init; } = string.Empty;
    public string FromEmail { get; init; } = "noreply@idelivery.com";
    public string FromName { get; init; } = "iDelivery";
    public bool UseSsl { get; init; } = true;
}