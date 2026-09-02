using IDelivery.Domain.Common.DomainEvents;
using IDelivery.Domain.Common.ValueObjects;

namespace IDelivery.Domain.Orders.Events;

public sealed class OrderDeliveredDomainEvent : DomainEvent
{
    public Guid OrderId { get; }
    public Guid TenantId { get; }
    public Guid CustomerId { get; }
    public Guid DeliveryDriverId { get; }
    public Money TotalAmount { get; }

    public OrderDeliveredDomainEvent(Guid orderId, Guid tenantId, Guid customerId, Guid deliveryDriverId, Money totalAmount)
    {
        OrderId = orderId;
        TenantId = tenantId;
        CustomerId = customerId;
        DeliveryDriverId = deliveryDriverId;
        TotalAmount = totalAmount;
    }
}