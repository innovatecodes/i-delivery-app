using IDelivery.Application.Abstractions.CQRS;

namespace IDelivery.Application.Commands.Carts;

public sealed record RemoveCartItemCommand(
    Guid ProductId) : ICommand;
