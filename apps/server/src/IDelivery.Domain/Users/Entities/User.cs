using IDelivery.SharedKernel.Common.Result;
using IDelivery.Domain.Roles;
using IDelivery.Domain.Users.Enums;
using IDelivery.Domain.Users.Events;
using IDelivery.Domain.Common.Entities;
using IDelivery.Domain.Common.ValueObjects;

namespace IDelivery.Domain.Users.Entities;

/// <summary>
/// Aggregate Root do Usuário.
/// Representa um usuário do sistema (cliente, entregador, admin, super admin).
/// Responsável por manter consistência das regras de negócio de identidade.
/// </summary>
public sealed class User : AggregateRoot
{
    public Email Email { get; private set; } = null!;
    public string? PasswordHash { get; private set; }
    public string FullName { get; private set; } = null!;
    public string? PhoneNumber { get; private set; }
    public Role Role { get; private set; }
    public Guid? TenantId { get; private set; }
    public UserStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public DateTime? ActivatedAt { get; private set; }
    public string? ActivationTokenHash { get; private set; }
    public DateTime? ActivationTokenExpiresAt { get; private set; }
    public string? ResetPasswordTokenHash { get; private set; }
    public DateTime? ResetPasswordTokenExpiresAt { get; private set; }

    private User() { }

    private User(
        Guid id,
        Email email,
        string? passwordHash,
        string fullName,
        Role role,
        Guid? tenantId = null,
        string? phoneNumber = null) : base(id)
    {
        Email = email;
        PasswordHash = passwordHash;
        FullName = fullName;
        Role = role;
        TenantId = tenantId;
        PhoneNumber = phoneNumber;
        Status = UserStatus.PendingActivation;
        CreatedAt = DateTime.UtcNow;

        AddDomainEvent(new UserRegisteredDomainEvent(Id, Email.Value, FullName, Role));
    }

    /// <summary>
    /// Factory method para criar um novo usuário.
    /// </summary>
    public static Result<User> Create(
        string email,
        string? passwordHash,
        string fullName,
        Role role,
        Guid? tenantId = null,
        string? phoneNumber = null)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result.Failure<User>(new Error("User.EmailRequired", "Email é obrigatório"));

        if (string.IsNullOrWhiteSpace(fullName))
            return Result.Failure<User>(new Error("User.FullNameRequired", "Nome completo é obrigatório"));

        if (fullName.Length > 200)
            return Result.Failure<User>(new Error("User.FullNameTooLong", "Nome completo deve ter no máximo 200 caracteres"));

        if (!Enum.IsDefined(typeof(Role), role))
            return Result.Failure<User>(new Error("User.InvalidRole", "Role inválido"));

        // Validações de tenant baseadas no role
        if (role.IsTenantScoped() && tenantId == null)
            return Result.Failure<User>(new Error("User.TenantRequired", "Usuário deve pertencer a um tenant"));

        if (role == Role.SuperAdmin && tenantId != null)
            return Result.Failure<User>(new Error("User.SuperAdminNoTenant", "Super Admin não pode ter tenant"));

        var user = new User(
            Guid.NewGuid(),
            Email.Create(email),
            passwordHash,
            fullName.Trim(),
            role,
            tenantId,
            phoneNumber?.Trim());

        return Result.Success(user);
    }

    /// <summary>
    /// Define o hash do token de ativação (gerado pela Application/Infrastructure).
    /// </summary>
    public void SetActivationToken(string tokenHash, DateTime expiresAt)
    {
        ActivationTokenHash = tokenHash;
        ActivationTokenExpiresAt = expiresAt;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Define o hash do token de reset de senha (gerado pela Application/Infrastructure).
    /// </summary>
    public void SetResetPasswordToken(string tokenHash, DateTime expiresAt)
    {
        ResetPasswordTokenHash = tokenHash;
        ResetPasswordTokenExpiresAt = expiresAt;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Ativa a conta do usuário (confirmação de email).
    /// Recebe o hash do token já calculado pela Application.
    /// </summary>
    public Result Activate(string tokenHash)
    {
        if (Status == UserStatus.Active)
            return Result.Failure(new Error("User.AlreadyActive", "Usuário já está ativo"));

        if (ActivationTokenHash == null || ActivationTokenHash != tokenHash)
            return Result.Failure(new Error("User.InvalidActivationToken", "Token de ativação inválido"));

        if (ActivationTokenExpiresAt == null || ActivationTokenExpiresAt < DateTime.UtcNow)
            return Result.Failure(new Error("User.ActivationTokenExpired", "Token de ativação expirado"));

        Status = UserStatus.Active;
        ActivatedAt = DateTime.UtcNow;
        ActivationTokenHash = null;
        ActivationTokenExpiresAt = null;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new UserActivatedDomainEvent(Id, Email.Value));

        return Result.Success();
    }

    /// <summary>
    /// Atualiza o perfil do usuário.
    /// </summary>
    public Result UpdateProfile(string fullName, string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return Result.Failure(new Error("User.FullNameRequired", "Nome completo é obrigatório"));

        if (fullName.Length > 200)
            return Result.Failure(new Error("User.FullNameTooLong", "Nome completo deve ter no máximo 200 caracteres"));

        FullName = fullName.Trim();
        PhoneNumber = phoneNumber?.Trim();
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new UserProfileUpdatedDomainEvent(Id, FullName, PhoneNumber));

        return Result.Success();
    }

    /// <summary>
    /// Altera a senha do usuário (recebe hash já gerado pela Application).
    /// </summary>
    public Result ChangePassword(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            return Result.Failure(new Error("User.PasswordHashRequired", "Hash da nova senha é obrigatório"));

        PasswordHash = passwordHash;
        UpdatedAt = DateTime.UtcNow;

        // Invalida tokens de reset de senha
        ResetPasswordTokenHash = null;
        ResetPasswordTokenExpiresAt = null;

        AddDomainEvent(new UserPasswordChangedDomainEvent(Id, Email.Value));

        return Result.Success();
    }

    /// <summary>
    /// Inicia processo de reset de senha (recebe token hash gerado pela Application).
    /// </summary>
    public Result RequestPasswordReset(string tokenHash, DateTime expiresAt)
    {
        if (Status != UserStatus.Active)
            return Result.Failure(new Error("User.NotActive", "Apenas usuários ativos podem resetar senha"));

        ResetPasswordTokenHash = tokenHash;
        ResetPasswordTokenExpiresAt = expiresAt;
        UpdatedAt = DateTime.UtcNow;

        // O evento precisa do token original para envio por email
        // Isso será tratado na Application layer
        AddDomainEvent(new UserPasswordResetRequestedDomainEvent(Id, Email.Value, string.Empty));

        return Result.Success();
    }

    /// <summary>
    /// Reseta a senha usando token.
    /// Recebe o hash do token já calculado pela Application.
    /// </summary>
    public Result ResetPassword(string tokenHash, string passwordHash)
    {
        if (ResetPasswordTokenHash == null || ResetPasswordTokenHash != tokenHash)
            return Result.Failure(new Error("User.InvalidResetToken", "Token de reset inválido"));

        if (ResetPasswordTokenExpiresAt == null || ResetPasswordTokenExpiresAt < DateTime.UtcNow)
            return Result.Failure(new Error("User.ResetTokenExpired", "Token de reset expirado"));

        return ChangePassword(passwordHash);
    }

    /// <summary>
    /// Registra login bem-sucedido.
    /// </summary>
    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Ativa o usuário (admin action).
    /// </summary>
    public Result ActivateByAdmin()
    {
        if (Status == UserStatus.Active)
            return Result.Failure(new Error("User.AlreadyActive", "Usuário já está ativo"));

        Status = UserStatus.Active;
        ActivatedAt = DateTime.UtcNow;
        ActivationTokenHash = null;
        ActivationTokenExpiresAt = null;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new UserActivatedDomainEvent(Id, Email.Value));

        return Result.Success();
    }

    /// <summary>
    /// Desativa o usuário (admin action).
    /// </summary>
    public Result Deactivate()
    {
        if (Status == UserStatus.Inactive)
            return Result.Failure(new Error("User.AlreadyInactive", "Usuário já está inativo"));

        Status = UserStatus.Inactive;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new UserDeactivatedDomainEvent(Id, Email.Value));

        return Result.Success();
    }

    /// <summary>
    /// Soft delete do usuário.
    /// </summary>
    public Result Delete()
    {
        if (Status == UserStatus.Deleted)
            return Result.Failure(new Error("User.AlreadyDeleted", "Usuário já foi excluído"));

        Status = UserStatus.Deleted;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new UserDeletedDomainEvent(Id, Email.Value));

        return Result.Success();
    }

    /// <summary>
    /// Altera o role do usuário (apenas SuperAdmin ou TenantAdmin).
    /// </summary>
    public Result ChangeRole(Role newRole, Guid? newTenantId = null)
    {
        if (!Enum.IsDefined(typeof(Role), newRole))
            return Result.Failure(new Error("User.InvalidRole", "Role inválido"));

        if (newRole.IsTenantScoped() && newTenantId == null)
            return Result.Failure(new Error("User.TenantRequired", "Usuário deve pertencer a um tenant"));

        if (newRole == Role.SuperAdmin && newTenantId != null)
            return Result.Failure(new Error("User.SuperAdminNoTenant", "Super Admin não pode ter tenant"));

        Role = newRole;
        TenantId = newTenantId;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new UserRoleChangedDomainEvent(Id, Email.Value, Role, TenantId));

        return Result.Success();
    }
}