using IDelivery.Application.Abstractions.Authentication;
using IDelivery.Application.Abstractions.CQRS;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.SharedKernel.Common.Result;

namespace IDelivery.Application.Queries.Orders;

public sealed class GetOrderQueryHandler : IQueryHandler<GetOrderQuery, OrderResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUser _currentUser;

    public GetOrderQueryHandler(
        IOrderRepository orderRepository,
        ITenantContext tenantContext,
        ICurrentUser currentUser)
    {
        _orderRepository = orderRepository;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
    }

    public async Task<Result<OrderResponse>> Handle(GetOrderQuery query, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantContext.TenantId;
        if (!tenantId.HasValue)
            return Result.Failure<OrderResponse>(new Error("Order.TenantRequired", "Tenant é obrigatório"));

        var order = await _orderRepository.GetByIdWithItemsAsync(query.OrderId, cancellationToken);
        if (order is null)
            return Result.Failure<OrderResponse>(new Error("Order.NotFound", "Pedido não encontrado"));

        if (order.TenantId != tenantId.Value)
            return Result.Failure<OrderResponse>(new Error("Order.AccessDenied", "Acesso negado"));

        var response = new OrderResponse(
            order.Id,
            order.TenantId,
            order.CustomerId,
            order.DeliveryDriverId,
            order.State,
            order.Items.Select(i => new OrderItemResponse(
                i.Id,
                i.ProductId,
                i.ProductName,
                i.UnitPrice,
                i.Currency,
                i.Quantity,
                i.GetSubtotal())).ToList(),
            order.Subtotal,
            order.DeliveryFee,
            order.TotalAmount,
            order.Currency,
            order.DeliveryAddress!,
            order.DeliveryDistanceKm,
            order.DeliveryFailureReasonDetail,
            order.CreatedAt,
            order.ConfirmedAt,
            order.PreparingAt,
            order.ReadyAt,
            order.OutForDeliveryAt,
            order.CompletedAt,
            order.CancelledAt);

        return Result.Success(response);
    }
}