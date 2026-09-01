using IDelivery.Application.Abstractions.CQRS;

namespace IDelivery.Application.Commands.Orders;

public sealed record ConfirmOrderCommand(
    Guid OrderId) : ICommand;