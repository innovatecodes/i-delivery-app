using IDelivery.Domain.Common.DomainEvents;

namespace IDelivery.Domain.Orders.Events;

public sealed class OrderDeliveredDomainEvent : DomainEvent
{
    public Guid OrderId { get; }
    public Guid TenantId { get; }
    public Guid CustomerId { get; }
    public Guid DeliveryDriverId { get; }

    public OrderDeliveredDomainEvent(Guid orderId, Guid tenantId, Guid customerId, Guid deliveryDriverId)
    {
        OrderId = orderId;
        TenantId = tenantId;
        CustomerId = customerId;
        DeliveryDriverId = deliveryDriverId;
    }
}