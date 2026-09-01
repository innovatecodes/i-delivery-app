using IDelivery.Application.Abstractions.CQRS;
using IDelivery.Application.Common.Models;
using IDelivery.Domain.Orders.Enums;

namespace IDelivery.Application.Queries.Orders;

public sealed record GetOrdersQuery(
    OrderState? State,
    int Page,
    int PageSize) : IQuery<PagedResult<OrderListItemResponse>>;