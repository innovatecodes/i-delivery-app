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
}