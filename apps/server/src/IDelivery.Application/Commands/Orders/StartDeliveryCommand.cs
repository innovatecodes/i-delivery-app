using IDelivery.Application.Abstractions.CQRS;

namespace IDelivery.Application.Commands.Orders;

public sealed record StartDeliveryCommand(
    Guid OrderId,
    Guid DeliveryDriverId) : ICommand;