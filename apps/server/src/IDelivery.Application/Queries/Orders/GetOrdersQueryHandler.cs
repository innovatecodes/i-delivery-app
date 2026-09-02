using IDelivery.Application.Abstractions.Authentication;
using IDelivery.Application.Abstractions.CQRS;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.Application.Common.Models;
using IDelivery.Domain.Orders.Enums;
using IDelivery.SharedKernel.Common.Result;

namespace IDelivery.Application.Queries.Orders;

public sealed class GetOrdersQueryHandler : IQueryHandler<GetOrdersQuery, PagedResult<OrderListItemResponse>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ITenantContext _tenantContext;

    public GetOrdersQueryHandler(
        IOrderRepository orderRepository,
        ITenantContext tenantContext)
    {
        _orderRepository = orderRepository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<PagedResult<OrderListItemResponse>>> Handle(GetOrdersQuery query, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantContext.TenantId;
        if (!tenantId.HasValue)
            return Result.Failure<PagedResult<OrderListItemResponse>>(new Error("Order.TenantRequired", "Tenant é obrigatório"));

        var page = query.Page > 0 ? query.Page : 1;
        var pageSize = query.PageSize > 0 && query.PageSize <= 100 ? query.PageSize : 20;

        var orders = await _orderRepository.GetByTenantIdAsync(tenantId.Value, query.State, page, pageSize, cancellationToken);
        var totalCount = await _orderRepository.CountByTenantIdAsync(tenantId.Value, query.State, cancellationToken);

        var items = orders.Select(o => new OrderListItemResponse(
            o.Id,
            o.CustomerId,
            o.State,
            o.TotalAmount.Amount,
            o.Subtotal.Currency,
            o.CreatedAt,
            o.CompletedAt)).ToList();

        var result = new PagedResult<OrderListItemResponse>(items, totalCount, page, pageSize);

        return Result.Success(result);
    }
}