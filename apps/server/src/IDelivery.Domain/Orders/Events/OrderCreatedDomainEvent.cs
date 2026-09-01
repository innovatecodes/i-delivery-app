using IDelivery.Domain.Common.DomainEvents;

namespace IDelivery.Domain.Orders.Events;

public sealed class OrderCreatedDomainEvent : DomainEvent
{
    public Guid OrderId { get; }
    public Guid TenantId { get; }
    public Guid CustomerId { get; }
    public decimal TotalAmount { get; }
    public string Currency { get; }

    public OrderCreatedDomainEvent(Guid orderId, Guid tenantId, Guid customerId, decimal totalAmount, string currency)
    {
        OrderId = orderId;
        TenantId = tenantId;
        CustomerId = customerId;
        TotalAmount = totalAmount;
        Currency = currency;
    }
}