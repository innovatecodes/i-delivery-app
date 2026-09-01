using IDelivery.Application.Abstractions.CQRS;

namespace IDelivery.Application.Commands.Orders;

public sealed record StartPreparingOrderCommand(
    Guid OrderId) : ICommand;