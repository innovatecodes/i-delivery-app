using IDelivery.Domain.Delivery.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IDelivery.Infrastructure.Persistence.Configurations;

public class DeliverySettingsConfiguration : IEntityTypeConfiguration<DeliverySettings>
{
    public void Configure(EntityTypeBuilder<DeliverySettings> builder)
    {
        builder.ToTable("DeliverySettings");

        builder.HasKey(ds => ds.Id);

        builder.Property(ds => ds.TenantId)
            .IsRequired();

        builder.Property(ds => ds.FeeType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(ds => ds.FixedFee)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(ds => ds.FreeAboveAmount)
            .HasPrecision(18, 2)
            .IsRequired(false);

        builder.Property(ds => ds.FeePerKm)
            .HasPrecision(18, 2)
            .IsRequired(false);

        builder.Property(ds => ds.MinimumFee)
            .HasPrecision(18, 2)
            .IsRequired(false);

        builder.Property(ds => ds.MaximumFee)
            .HasPrecision(18, 2)
            .IsRequired(false);

        builder.Property(ds => ds.IsActive)
            .IsRequired();

        builder.Property(ds => ds.CreatedAt)
            .IsRequired();

        builder.Property(ds => ds.UpdatedAt)
            .IsRequired(false);

        builder.HasIndex(ds => ds.TenantId)
            .HasDatabaseName("IX_DeliverySettings_TenantId")
            .IsUnique();
    }
}