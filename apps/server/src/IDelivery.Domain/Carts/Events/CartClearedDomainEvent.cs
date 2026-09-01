using IDelivery.Domain.Common.DomainEvents;

namespace IDelivery.Domain.Carts.Events;

public sealed class CartClearedDomainEvent : DomainEvent
{
    public Guid CartId { get; }
    public Guid TenantId { get; }

    public CartClearedDomainEvent(Guid cartId, Guid tenantId)
    {
        CartId = cartId;
        TenantId = tenantId;
    }
}
