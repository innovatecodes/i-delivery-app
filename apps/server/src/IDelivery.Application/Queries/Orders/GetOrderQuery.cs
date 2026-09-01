using IDelivery.Application.Abstractions.CQRS;

namespace IDelivery.Application.Queries.Orders;

public sealed record GetOrderQuery(
    Guid OrderId) : IQuery<OrderResponse>;