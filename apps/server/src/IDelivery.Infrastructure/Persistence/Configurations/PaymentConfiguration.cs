using IDelivery.Domain.Payments.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IDelivery.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.OrderId)
            .IsRequired();

        builder.Property(p => p.TenantId)
            .IsRequired();

        builder.Property(p => p.CustomerId)
            .IsRequired();

        builder.Property(p => p.Method)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.PaidAt)
            .IsRequired(false);

        builder.OwnsOne(p => p.Amount, amountBuilder =>
        {
            amountBuilder.Property(a => a.Amount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            amountBuilder.Property(a => a.Currency)
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.HasIndex(p => p.OrderId)
            .IsUnique();

        builder.HasIndex(p => p.TenantId);
        builder.HasIndex(p => p.CustomerId);
    }
}
