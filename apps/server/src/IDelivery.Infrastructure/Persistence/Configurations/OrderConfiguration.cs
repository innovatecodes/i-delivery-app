using IDelivery.Domain.Orders.Entities;
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

        builder.Property(o => o.Subtotal)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(o => o.DeliveryFee)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(o => o.TotalAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(o => o.Currency)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(o => o.DeliveryAddress)
            .HasMaxLength(500)
            .IsRequired();

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