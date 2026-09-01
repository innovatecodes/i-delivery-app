using IDelivery.Domain.Common.DomainEvents;

namespace IDelivery.Domain.Catalog.Events;

public sealed class CategoryCreatedDomainEvent : DomainEvent
{
    public Guid CategoryId { get; }
    public Guid TenantId { get; }
    public string Name { get; }

    public CategoryCreatedDomainEvent(Guid categoryId, Guid tenantId, string name)
    {
        CategoryId = categoryId;
        TenantId = tenantId;
        Name = name;
    }
}
