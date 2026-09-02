using IDelivery.Domain.Common.DomainEvents;
using IDelivery.Domain.Common.ValueObjects;

namespace IDelivery.Domain.Orders.Events;

public sealed class OrderCreatedDomainEvent : DomainEvent
{
    public Guid OrderId { get; }
    public Guid TenantId { get; }
    public Guid CustomerId { get; }
    public Money TotalAmount { get; }

    public OrderCreatedDomainEvent(Guid orderId, Guid tenantId, Guid customerId, Money totalAmount)
    {
        OrderId = orderId;
        TenantId = tenantId;
        CustomerId = customerId;
        TotalAmount = totalAmount;
    }
}