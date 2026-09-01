using IDelivery.Application.Abstractions.CQRS;

namespace IDelivery.Application.Commands.Orders;

public sealed record CancelOrderCommand(
    Guid OrderId,
    string? CancelledBy) : ICommand;