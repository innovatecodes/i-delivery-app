using IDelivery.Domain.Customers.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IDelivery.Infrastructure.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.TenantId)
            .IsRequired();

        builder.Property(c => c.UserId)
            .IsRequired();

        builder.Property(c => c.FullName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.Email)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.PhoneNumber)
            .HasMaxLength(20)
            .IsRequired(false);

        builder.Property(c => c.Notes)
            .HasMaxLength(1000)
            .IsRequired(false);

        builder.Property(c => c.IsActive)
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .IsRequired(false);

        builder.HasIndex(c => new { c.TenantId, c.UserId })
            .HasDatabaseName("IX_Customers_TenantUser")
            .IsUnique();

        builder.HasIndex(c => new { c.TenantId, c.Email })
            .HasDatabaseName("IX_Customers_TenantEmail")
            .IsUnique();

        builder.Ignore(c => c.Addresses);
    }
}
