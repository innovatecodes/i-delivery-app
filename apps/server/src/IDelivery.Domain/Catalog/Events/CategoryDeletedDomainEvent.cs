using IDelivery.Domain.Common.DomainEvents;

namespace IDelivery.Domain.Catalog.Events;

public sealed class CategoryDeletedDomainEvent : DomainEvent
{
    public Guid CategoryId { get; }
    public Guid TenantId { get; }

    public CategoryDeletedDomainEvent(Guid categoryId, Guid tenantId)
    {
        CategoryId = categoryId;
        TenantId = tenantId;
    }
}
