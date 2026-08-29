// Bounded Context: Roles (Autorização e Permissões)
// Extensões de domínio para o enum Role.
// Separadas do enum seguindo responsabilidade única - extensões não fazem parte do conceito de negócio core.

using IDelivery.Domain.Roles.Enums;

namespace IDelivery.Domain.Roles;

/// <summary>
/// Métodos de extensão para o enum Role.
/// Fornecem comportamento auxiliar sem poluir o enum core.
/// Mantidos no Domain por serem regras de negócio sobre roles.
/// </summary>
public static class RoleExtensions
{
    /// <summary>
    /// Verifica se o role é de nível administrativo (SuperAdmin ou TenantAdmin).
    /// </summary>
    public static bool IsAdmin(this Role role)
    {
        return role == Role.SuperAdmin || role == Role.TenantAdmin;
    }

    /// <summary>
    /// Verifica se o role pertence a um tenant (não é SuperAdmin).
    /// </summary>
    public static bool IsTenantScoped(this Role role)
    {
        return role != Role.SuperAdmin;
    }

    /// <summary>
    /// Retorna a descrição amigável do role.
    /// </summary>
    public static string GetDescription(this Role role)
    {
        return role switch
        {
            Role.SuperAdmin => "Super Administrador",
            Role.TenantAdmin => "Administrador do Tenant",
            Role.Delivery => "Entregador",
            Role.Customer => "Cliente",
            _ => role.ToString()
        };
    }
}