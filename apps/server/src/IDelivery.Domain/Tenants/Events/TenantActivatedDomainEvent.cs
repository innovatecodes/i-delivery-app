using IDelivery.Domain.Common.DomainEvents;

namespace IDelivery.Domain.Tenants.Events;

/// <summary>
/// Evento disparado quando um Tenant é ativado.
/// </summary>
public sealed class TenantActivatedDomainEvent : DomainEvent
{
    public Guid TenantId { get; }

    public TenantActivatedDomainEvent(Guid tenantId)
    {
        TenantId = tenantId;
    }
}