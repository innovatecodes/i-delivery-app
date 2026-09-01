using IDelivery.Domain.Common.DomainEvents;

namespace IDelivery.Domain.Carts.Events;

public sealed class CartCreatedDomainEvent : DomainEvent
{
    public Guid CartId { get; }
    public Guid TenantId { get; }
    public Guid? UserId { get; }

    public CartCreatedDomainEvent(Guid cartId, Guid tenantId, Guid? userId)
    {
        CartId = cartId;
        TenantId = tenantId;
        UserId = userId;
    }
}
