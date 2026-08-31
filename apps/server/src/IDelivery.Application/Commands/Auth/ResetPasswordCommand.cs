using IDelivery.Application.Abstractions.CQRS;

namespace IDelivery.Application.Commands.Auth;

public sealed record ResetPasswordCommand(
    string Token,
    string NewPassword) : ICommand;