using IDelivery.Domain.Customers.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IDelivery.Infrastructure.Persistence.Configurations;

public class CustomerAddressConfiguration : IEntityTypeConfiguration<CustomerAddress>
{
    public void Configure(EntityTypeBuilder<CustomerAddress> builder)
    {
        builder.ToTable("CustomerAddresses");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.CustomerId)
            .IsRequired();

        builder.Property(a => a.Label)
            .HasMaxLength(50)
            .IsRequired();

        builder.OwnsOne(a => a.Address, address =>
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

        builder.Property(a => a.IsDefault)
            .IsRequired();

        builder.Property(a => a.CreatedAt)
            .IsRequired();

        builder.Property(a => a.UpdatedAt)
            .IsRequired(false);

        builder.HasIndex(a => new { a.CustomerId, a.Label })
            .HasDatabaseName("IX_CustomerAddresses_CustomerLabel")
            .IsUnique();

        builder.HasOne<Customer>()
            .WithMany()
            .HasForeignKey(a => a.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}