using IDelivery.Domain.Common.DomainEvents;

namespace IDelivery.Domain.Catalog.Events;

public sealed class ProductUpdatedDomainEvent : DomainEvent
{
    public Guid ProductId { get; }
    public Guid TenantId { get; }
    public string Name { get; }

    public ProductUpdatedDomainEvent(Guid productId, Guid tenantId, string name)
    {
        ProductId = productId;
        TenantId = tenantId;
        Name = name;
    }
}
