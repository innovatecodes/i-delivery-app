namespace IDelivery.Domain.Roles;

/// <summary>
/// Papéis fundamentais do sistema.
/// Representa conceitos de negócio estáveis - Enum aceitável no Domain (Eric Evans).
/// A modelagem permite evolução futura (novos roles) sem breaking changes.
/// </summary>
public enum Role
{
    /// <summary>
    /// Super Administrador do SaaS - acesso total a todos os tenants e configurações globais.
    /// </summary>
    SuperAdmin = 1,

    /// <summary>
    /// Administrador do Tenant - gerencia seu próprio tenant (produtos, pedidos, entregadores, etc).
    /// </summary>
    TenantAdmin = 2,

    /// <summary>
    /// Entregador - visualiza e gerencia suas entregas atribuídas.
    /// </summary>
    Delivery = 3,

    /// <summary>
    /// Cliente final - faz pedidos, acompanha status, gerencia seu perfil/endereços.
    /// </summary>
    Customer = 4
}