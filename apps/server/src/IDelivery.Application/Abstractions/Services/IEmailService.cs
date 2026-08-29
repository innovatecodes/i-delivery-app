namespace IDelivery.Application.Abstractions.Services;

public interface IEmailService
{
    Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
    Task SendActivationEmailAsync(string to, string activationLink, CancellationToken cancellationToken = default);
}