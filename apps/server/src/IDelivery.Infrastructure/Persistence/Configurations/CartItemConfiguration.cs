using IDelivery.Domain.Carts.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IDelivery.Infrastructure.Persistence.Configurations;

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("CartItems");

        builder.HasKey(ci => ci.Id);

        builder.Property(ci => ci.CartId)
            .IsRequired();

        builder.Property(ci => ci.ProductId)
            .IsRequired();

        builder.Property(ci => ci.ProductName)
            .HasMaxLength(200)
            .IsRequired();

        builder.OwnsOne(ci => ci.UnitPrice, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("UnitPrice")
                .HasPrecision(18, 2)
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("Currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.Property(ci => ci.Quantity)
            .IsRequired();

        builder.Property(ci => ci.CreatedAt)
            .IsRequired();

        builder.Property(ci => ci.UpdatedAt)
            .IsRequired(false);

        builder.HasIndex(ci => new { ci.CartId, ci.ProductId })
            .HasDatabaseName("IX_CartItems_CartProduct")
            .IsUnique();

        builder.HasOne<Cart>()
            .WithMany()
            .HasForeignKey(ci => ci.CartId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}