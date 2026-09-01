using IDelivery.Domain.Common.DomainEvents;

namespace IDelivery.Domain.Carts.Events;

public sealed class CartItemAddedDomainEvent : DomainEvent
{
    public Guid CartId { get; }
    public Guid TenantId { get; }
    public Guid ProductId { get; }
    public int Quantity { get; }

    public CartItemAddedDomainEvent(Guid cartId, Guid tenantId, Guid productId, int quantity)
    {
        CartId = cartId;
        TenantId = tenantId;
        ProductId = productId;
        Quantity = quantity;
    }
}
