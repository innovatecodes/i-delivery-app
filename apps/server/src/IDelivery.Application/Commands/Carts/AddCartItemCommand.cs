using IDelivery.Application.Abstractions.CQRS;

namespace IDelivery.Application.Commands.Carts;

public sealed record AddCartItemCommand(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    string Currency,
    int Quantity = 1) : ICommand;
