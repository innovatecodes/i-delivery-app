using IDelivery.Domain.Common.DomainEvents;

namespace IDelivery.Domain.Carts.Events;

public sealed class CartItemRemovedDomainEvent : DomainEvent
{
    public Guid CartId { get; }
    public Guid TenantId { get; }
    public Guid ProductId { get; }

    public CartItemRemovedDomainEvent(Guid cartId, Guid tenantId, Guid productId)
    {
        CartId = cartId;
        TenantId = tenantId;
        ProductId = productId;
    }
}
