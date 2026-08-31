namespace IDelivery.Domain.Users.Enums;

/// <summary>
/// Status possível de um usuário no sistema.
/// </summary>
public enum UserStatus
{
    /// <summary>Usuário ativo e pode acessar o sistema.</summary>
    Active = 1,

    /// <summary>Usuário inativo/bloqueado (não pode acessar).</summary>
    Inactive = 2,

    /// <summary>Usuário pendente de ativação (ex: email não confirmado).</summary>
    PendingActivation = 3,

    /// <summary>Usuário excluído (soft delete).</summary>
    Deleted = 4
}