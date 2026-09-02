using IDelivery.Application.Abstractions.CQRS;

namespace IDelivery.Application.Commands.Auth;

public sealed record RegisterCommand(
    string Email,
    string Password,
    string FullName,
    string? PhoneNumber = null) : ICommand<Guid>;
