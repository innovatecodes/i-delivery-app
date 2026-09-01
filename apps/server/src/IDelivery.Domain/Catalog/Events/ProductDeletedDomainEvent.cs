using IDelivery.Domain.Common.DomainEvents;

namespace IDelivery.Domain.Catalog.Events;

public sealed class ProductDeletedDomainEvent : DomainEvent
{
    public Guid ProductId { get; }
    public Guid TenantId { get; }

    public ProductDeletedDomainEvent(Guid productId, Guid tenantId)
    {
        ProductId = productId;
        TenantId = tenantId;
    }
}
