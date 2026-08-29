// Bounded Context: Tenants
// Enums específicos do contexto de Tenants.

namespace IDelivery.Domain.Tenants.Enums;

/// <summary>
/// Status possível de um Tenant no sistema.
/// </summary>
public enum TenantStatus
{
    /// <summary>Tenant ativo e operando normalmente.</summary>
    Active = 1,

    /// <summary>Tenant bloqueado (não pode operar).</summary>
    Blocked = 2,

    /// <summary>Tenant em período de trial (7 dias grátis).</summary>
    Trial = 3,

    /// <summary>Tenant expirado (trial acabou, não pagou).</summary>
    Expired = 4
}