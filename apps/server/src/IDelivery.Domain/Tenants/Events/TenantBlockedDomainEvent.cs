using IDelivery.Domain.Common.DomainEvents;

namespace IDelivery.Domain.Tenants.Events;

/// <summary>
/// Evento disparado quando um Tenant é bloqueado.
/// </summary>
public sealed class TenantBlockedDomainEvent : DomainEvent
{
    public Guid TenantId { get; }

    public TenantBlockedDomainEvent(Guid tenantId)
    {
        TenantId = tenantId;
    }
}