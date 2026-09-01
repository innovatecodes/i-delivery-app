using IDelivery.Application.Abstractions.CQRS;

namespace IDelivery.Application.Commands.Orders;

public sealed record DeliverOrderCommand(
    Guid OrderId) : ICommand;