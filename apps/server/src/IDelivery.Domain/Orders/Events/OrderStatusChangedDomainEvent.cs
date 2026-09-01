using IDelivery.Domain.Common.DomainEvents;
using IDelivery.Domain.Orders.Enums;

namespace IDelivery.Domain.Orders.Events;

public sealed class OrderStatusChangedDomainEvent : DomainEvent
{
    public Guid OrderId { get; }
    public Guid TenantId { get; }
    public OrderState FromState { get; }
    public OrderState ToState { get; }

    public OrderStatusChangedDomainEvent(Guid orderId, Guid tenantId, OrderState fromState, OrderState toState)
    {
        OrderId = orderId;
        TenantId = tenantId;
        FromState = fromState;
        ToState = toState;
    }
}