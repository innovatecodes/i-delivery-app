using IDelivery.Domain.Common.DomainEvents;

namespace IDelivery.Domain.Payments.Events;

public sealed class PaymentMarkedAsPaidDomainEvent : DomainEvent
{
    public Guid PaymentId { get; }
    public Guid OrderId { get; }
    public Guid TenantId { get; }
    public Guid CustomerId { get; }

    public PaymentMarkedAsPaidDomainEvent(
        Guid paymentId, Guid orderId, Guid tenantId, Guid customerId)
    {
        PaymentId = paymentId;
        OrderId = orderId;
        TenantId = tenantId;
        CustomerId = customerId;
    }
}
