using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IDelivery.Domain.Common.ValueObjects;
using IDelivery.Domain.Tenants.Entities;
using IDelivery.Domain.Tenants.Enums;

namespace IDelivery.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuração EF Core para a entidade Tenant.
/// Mapeia Value Objects como owned types e enums como string/int.
/// </summary>
public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("tenants", schema: "public");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(t => t.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.Slug)
            .HasColumnName("slug")
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(t => t.Slug)
            .IsUnique()
            .HasDatabaseName("ix_tenants_slug");

        builder.Property(t => t.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        builder.Property(t => t.LogoUrl)
            .HasColumnName("logo_url")
            .HasMaxLength(500);

        builder.Property(t => t.Status)
            .HasColumnName("status")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at");

        // Address como Owned Type
        builder.OwnsOne(t => t.Address, addressBuilder =>
        {
            addressBuilder.Property(a => a.Street)
                .HasColumnName("address_street")
                .HasMaxLength(200);

            addressBuilder.Property(a => a.Number)
                .HasColumnName("address_number")
                .HasMaxLength(20);

            addressBuilder.Property(a => a.Complement)
                .HasColumnName("address_complement")
                .HasMaxLength(100);

            addressBuilder.Property(a => a.Neighborhood)
                .HasColumnName("address_neighborhood")
                .HasMaxLength(100);

            addressBuilder.Property(a => a.City)
                .HasColumnName("address_city")
                .HasMaxLength(100);

            addressBuilder.Property(a => a.State)
                .HasColumnName("address_state")
                .HasMaxLength(2);

            addressBuilder.OwnsOne(a => a.ZipCode, zipBuilder =>
            {
                zipBuilder.Property(z => z.Value)
                    .HasColumnName("address_zip_code")
                    .HasMaxLength(9)
                    .IsRequired();

                zipBuilder.Property(z => z.DigitsOnly)
                    .HasColumnName("address_zip_code_digits")
                    .HasMaxLength(8);
            });

            addressBuilder.Property(a => a.Reference)
                .HasColumnName("address_reference")
                .HasMaxLength(200);
        });

        // Email como Owned Type
        builder.OwnsOne(t => t.Email, emailBuilder =>
        {
            emailBuilder.Property(e => e.Value)
                .HasColumnName("email")
                .HasMaxLength(255);
        });

        // Phone como Owned Type
        builder.OwnsOne(t => t.Phone, phoneBuilder =>
        {
            phoneBuilder.Property(p => p.Value)
                .HasColumnName("phone")
                .HasMaxLength(20);
        });

        // WhatsApp como Owned Type
        builder.OwnsOne(t => t.WhatsApp, whatsAppBuilder =>
        {
            whatsAppBuilder.Property(w => w.Value)
                .HasColumnName("whatsapp")
                .HasMaxLength(20);
        });

        // Domain Events não são persistidos (ignorados)
        builder.Ignore(t => t.DomainEvents);
    }
}