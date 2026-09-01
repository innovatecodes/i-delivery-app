using IDelivery.Domain.Common.DomainEvents;
using IDelivery.Domain.Orders.Enums;

namespace IDelivery.Domain.Orders.Events;

public sealed class OrderDeliveryFailedDomainEvent : DomainEvent
{
    public Guid OrderId { get; }
    public Guid TenantId { get; }
    public Guid CustomerId { get; }
    public Guid DeliveryDriverId { get; }
    public DeliveryFailureReason Reason { get; }
    public string? ReasonDetail { get; }

    public OrderDeliveryFailedDomainEvent(
        Guid orderId,
        Guid tenantId,
        Guid customerId,
        Guid deliveryDriverId,
        DeliveryFailureReason reason,
        string? reasonDetail)
    {
        OrderId = orderId;
        TenantId = tenantId;
        CustomerId = customerId;
        DeliveryDriverId = deliveryDriverId;
        Reason = reason;
        ReasonDetail = reasonDetail;
    }
}