using IDelivery.Application.Abstractions.CQRS;

namespace IDelivery.Application.Commands.Carts;

public sealed record UpdateCartItemQuantityCommand(
    Guid ProductId,
    int Quantity) : ICommand;
