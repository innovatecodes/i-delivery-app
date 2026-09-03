namespace IDelivery.Application.Abstractions.Messaging;

public interface IEmailService
{
    Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
}