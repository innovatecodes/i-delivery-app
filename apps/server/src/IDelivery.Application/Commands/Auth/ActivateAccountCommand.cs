using IDelivery.Application.Abstractions.CQRS;

namespace IDelivery.Application.Commands.Auth;

public sealed record ActivateAccountCommand(
    string Token) : ICommand;