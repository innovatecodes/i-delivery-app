using IDelivery.Application.Abstractions.CQRS;
using IDelivery.Domain.Orders.Enums;

namespace IDelivery.Application.Commands.Orders;

public sealed record FailDeliveryCommand(
    Guid OrderId,
    DeliveryFailureReason Reason,
    string? ReasonDetail) : ICommand;