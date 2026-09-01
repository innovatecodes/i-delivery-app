using IDelivery.Domain.Common.DomainEvents;

namespace IDelivery.Domain.Orders.Events;

public sealed class OrderCancelledDomainEvent : DomainEvent
{
    public Guid OrderId { get; }
    public Guid TenantId { get; }
    public Guid CustomerId { get; }
    public string? CancelledBy { get; }

    public OrderCancelledDomainEvent(Guid orderId, Guid tenantId, Guid customerId, string? cancelledBy)
    {
        OrderId = orderId;
        TenantId = tenantId;
        CustomerId = customerId;
        CancelledBy = cancelledBy;
    }
}