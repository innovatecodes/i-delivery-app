using IDelivery.Application.Abstractions.CQRS;
using IDelivery.Domain.Orders.Entities;

namespace IDelivery.Application.Commands.Orders;

public sealed record CreateOrderCommand(
    List<CreateOrderItemDto> Items,
    decimal DeliveryFee,
    string Currency,
    string DeliveryAddress,
    decimal? DeliveryDistanceKm) : ICommand<Guid>;

public sealed record CreateOrderItemDto(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    string Currency,
    int Quantity);