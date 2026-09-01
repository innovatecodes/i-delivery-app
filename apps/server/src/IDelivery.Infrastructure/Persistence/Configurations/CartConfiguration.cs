using IDelivery.Domain.Carts.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IDelivery.Infrastructure.Persistence.Configurations;

public class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.ToTable("Carts");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.TenantId)
            .IsRequired();

        builder.Property(c => c.UserId)
            .IsRequired(false);

        builder.Property(c => c.SessionId)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .IsRequired(false);

        builder.HasIndex(c => new { c.TenantId, c.UserId })
            .HasDatabaseName("IX_Carts_TenantUser")
            .IsUnique()
            .HasFilter("[UserId] IS NOT NULL");

        builder.HasIndex(c => new { c.TenantId, c.SessionId })
            .HasDatabaseName("IX_Carts_TenantSession")
            .IsUnique()
            .HasFilter("[SessionId] IS NOT NULL");

        builder.Ignore(c => c.Items);
    }
}
