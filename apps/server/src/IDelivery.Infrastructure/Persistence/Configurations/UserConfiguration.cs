using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IDelivery.Domain.Common.ValueObjects;
using IDelivery.Domain.Users.Entities;
using IDelivery.Domain.Users.Enums;
using IDelivery.Domain.Roles;

namespace IDelivery.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuração EF Core para a entidade User.
/// Mapeia Value Objects como owned types e enums como int.
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users", schema: "public");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(u => u.FullName)
            .HasColumnName("full_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(u => u.PhoneNumber)
            .HasColumnName("phone_number")
            .HasMaxLength(20);

        builder.Property(u => u.Role)
            .HasColumnName("role")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(u => u.TenantId)
            .HasColumnName("tenant_id");

        builder.Property(u => u.Status)
            .HasColumnName("status")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(u => u.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(u => u.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(u => u.LastLoginAt)
            .HasColumnName("last_login_at");

        builder.Property(u => u.ActivatedAt)
            .HasColumnName("activated_at");

        builder.Property(u => u.ActivationTokenHash)
            .HasColumnName("activation_token_hash")
            .HasMaxLength(500);

        builder.Property(u => u.ActivationTokenExpiresAt)
            .HasColumnName("activation_token_expires_at");

        builder.Property(u => u.ResetPasswordTokenHash)
            .HasColumnName("reset_password_token_hash")
            .HasMaxLength(500);

        builder.Property(u => u.ResetPasswordTokenExpiresAt)
            .HasColumnName("reset_password_token_expires_at");

        // Email como Owned Type
        builder.OwnsOne(u => u.Email, emailBuilder =>
        {
            emailBuilder.Property(e => e.Value)
                .HasColumnName("email")
                .HasMaxLength(255)
                .IsRequired();

            emailBuilder.HasIndex(e => e.Value)
                .IsUnique()
                .HasDatabaseName("ix_users_email");
        });

        // PasswordHash como propriedade direta
        builder.Property(u => u.PasswordHash)
            .HasColumnName("password_hash")
            .HasMaxLength(500);

        // Domain Events não são persistidos
        builder.Ignore(u => u.DomainEvents);
    }
}