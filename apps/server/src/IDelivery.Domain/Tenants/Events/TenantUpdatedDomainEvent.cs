using IDelivery.Domain.Common.DomainEvents;

namespace IDelivery.Domain.Tenants.Events;

/// <summary>
/// Evento disparado quando os detalhes do Tenant são atualizados.
/// </summary>
public sealed class TenantUpdatedDomainEvent : DomainEvent
{
    public Guid TenantId { get; }
    public string Name { get; }

    public TenantUpdatedDomainEvent(Guid tenantId, string name)
    {
        TenantId = tenantId;
        Name = name;
    }
}