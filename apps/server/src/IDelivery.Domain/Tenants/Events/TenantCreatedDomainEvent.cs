using IDelivery.Domain.Common.DomainEvents;

namespace IDelivery.Domain.Tenants.Events;

/// <summary>
/// Evento disparado quando um novo Tenant é criado.
/// </summary>
public sealed class TenantCreatedDomainEvent : DomainEvent
{
    public Guid TenantId { get; }
    public string Name { get; }
    public string Slug { get; }

    public TenantCreatedDomainEvent(Guid tenantId, string name, string slug)
    {
        TenantId = tenantId;
        Name = name;
        Slug = slug;
    }
}