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

        builder.OwnsOne(ds => ds.FixedFee, feeBuilder =>
        {
            feeBuilder.Property(f => f.Amount)
                .HasColumnName("fixed_fee")
                .HasPrecision(18, 2)
                .IsRequired();

            feeBuilder.Property(f => f.Currency)
                .HasColumnName("fixed_fee_currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.OwnsOne(ds => ds.FreeAboveAmount, amountBuilder =>
        {
            amountBuilder.Property(a => a.Amount)
                .HasColumnName("free_above_amount")
                .HasPrecision(18, 2);

            amountBuilder.Property(a => a.Currency)
                .HasColumnName("free_above_amount_currency")
                .HasMaxLength(3);
        });

        builder.OwnsOne(ds => ds.FeePerKm, feeBuilder =>
        {
            feeBuilder.Property(f => f.Amount)
                .HasColumnName("fee_per_km")
                .HasPrecision(18, 2);

            feeBuilder.Property(f => f.Currency)
                .HasColumnName("fee_per_km_currency")
                .HasMaxLength(3);
        });

        builder.OwnsOne(ds => ds.MinimumFee, feeBuilder =>
        {
            feeBuilder.Property(f => f.Amount)
                .HasColumnName("minimum_fee")
                .HasPrecision(18, 2);

            feeBuilder.Property(f => f.Currency)
                .HasColumnName("minimum_fee_currency")
                .HasMaxLength(3);
        });

        builder.OwnsOne(ds => ds.MaximumFee, feeBuilder =>
        {
            feeBuilder.Property(f => f.Amount)
                .HasColumnName("maximum_fee")
                .HasPrecision(18, 2);

            feeBuilder.Property(f => f.Currency)
                .HasColumnName("maximum_fee_currency")
                .HasMaxLength(3);
        });

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