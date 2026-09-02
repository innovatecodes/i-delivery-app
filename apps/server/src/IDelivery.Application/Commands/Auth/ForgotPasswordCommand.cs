using IDelivery.Application.Abstractions.CQRS;

namespace IDelivery.Application.Commands.Auth;

public sealed record ForgotPasswordCommand(
    string Email) : ICommand;
