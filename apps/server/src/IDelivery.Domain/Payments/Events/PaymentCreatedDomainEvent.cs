using IDelivery.Domain.Common.DomainEvents;
using IDelivery.Domain.Common.ValueObjects;
using IDelivery.Domain.Payments.Enums;

namespace IDelivery.Domain.Payments.Events;

public sealed class PaymentCreatedDomainEvent : DomainEvent
{
    public Guid PaymentId { get; }
    public Guid OrderId { get; }
    public Guid TenantId { get; }
    public Guid CustomerId { get; }
    public Money Amount { get; }
    public PaymentMethod Method { get; }

    public PaymentCreatedDomainEvent(
        Guid paymentId, Guid orderId, Guid tenantId, Guid customerId,
        Money amount, PaymentMethod method)
    {
        PaymentId = paymentId;
        OrderId = orderId;
        TenantId = tenantId;
        CustomerId = customerId;
        Amount = amount;
        Method = method;
    }
}
