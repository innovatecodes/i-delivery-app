using IDelivery.Domain.Orders.Entities;
using IDelivery.Domain.Common.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IDelivery.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.TenantId)
            .IsRequired();

        builder.Property(o => o.CustomerId)
            .IsRequired();

        builder.Property(o => o.DeliveryDriverId)
            .IsRequired(false);

        builder.Property(o => o.State)
            .HasConversion<int>()
            .IsRequired();

        builder.OwnsOne(o => o.Subtotal, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("Subtotal")
                .HasPrecision(18, 2)
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("Currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.OwnsOne(o => o.DeliveryFee, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("DeliveryFee")
                .HasPrecision(18, 2)
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("Currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.OwnsOne(o => o.TotalAmount, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("TotalAmount")
                .HasPrecision(18, 2)
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("Currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.OwnsOne(o => o.DeliveryAddress, address =>
        {
            address.Property(a => a.Street)
                .HasMaxLength(200)
                .IsRequired();

            address.Property(a => a.Number)
                .HasMaxLength(20)
                .IsRequired();

            address.Property(a => a.Complement)
                .HasMaxLength(100)
                .IsRequired(false);

            address.Property(a => a.Neighborhood)
                .HasMaxLength(100)
                .IsRequired();

            address.Property(a => a.City)
                .HasMaxLength(100)
                .IsRequired();

            address.Property(a => a.State)
                .HasMaxLength(2)
                .IsRequired();

            address.OwnsOne(a => a.ZipCode, zip =>
            {
                zip.Property(z => z.Value)
                    .HasColumnName("ZipCode")
                    .HasMaxLength(10)
                    .IsRequired();
            });

            address.Property(a => a.Reference)
                .HasMaxLength(200)
                .IsRequired(false);
        });

        builder.Property(o => o.DeliveryDistanceKm)
            .HasPrecision(10, 2)
            .IsRequired(false);

        builder.Property(o => o.DeliveryFailureReasonDetail)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(o => o.CreatedAt)
            .IsRequired();

        builder.Property(o => o.ConfirmedAt)
            .IsRequired(false);

        builder.Property(o => o.PreparingAt)
            .IsRequired(false);

        builder.Property(o => o.ReadyAt)
            .IsRequired(false);

        builder.Property(o => o.OutForDeliveryAt)
            .IsRequired(false);

        builder.Property(o => o.CompletedAt)
            .IsRequired(false);

        builder.Property(o => o.CancelledAt)
            .IsRequired(false);

        builder.HasIndex(o => new { o.TenantId, o.State })
            .HasDatabaseName("IX_Orders_TenantId_State");

        builder.HasIndex(o => new { o.TenantId, o.CustomerId })
            .HasDatabaseName("IX_Orders_TenantId_CustomerId");

        builder.HasIndex(o => new { o.TenantId, o.DeliveryDriverId })
            .HasDatabaseName("IX_Orders_TenantId_DeliveryDriverId")
            .HasFilter("[DeliveryDriverId] IS NOT NULL");

        builder.Ignore(o => o.Items);
    }
}