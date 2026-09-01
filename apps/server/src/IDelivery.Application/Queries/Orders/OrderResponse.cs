using IDelivery.Domain.Orders.Enums;

namespace IDelivery.Application.Queries.Orders;

public sealed record OrderResponse(
    Guid Id,
    Guid TenantId,
    Guid CustomerId,
    Guid? DeliveryDriverId,
    OrderState State,
    List<OrderItemResponse> Items,
    decimal Subtotal,
    decimal DeliveryFee,
    decimal TotalAmount,
    string Currency,
    string DeliveryAddress,
    decimal? DeliveryDistanceKm,
    string? DeliveryFailureReasonDetail,
    DateTime CreatedAt,
    DateTime? ConfirmedAt,
    DateTime? PreparingAt,
    DateTime? ReadyAt,
    DateTime? OutForDeliveryAt,
    DateTime? CompletedAt,
    DateTime? CancelledAt);

public sealed record OrderItemResponse(
    Guid Id,
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    string Currency,
    int Quantity,
    decimal Subtotal);

public sealed record OrderListItemResponse(
    Guid Id,
    Guid CustomerId,
    OrderState State,
    decimal TotalAmount,
    string Currency,
    DateTime CreatedAt,
    DateTime? CompletedAt);