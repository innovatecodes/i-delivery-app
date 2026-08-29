// Bounded Context: Tenants
// Domain Events específicos do contexto de Tenants.
// Representam fatos ocorridos no domínio que outras partes do sistema podem reagir.

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