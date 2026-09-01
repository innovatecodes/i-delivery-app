using Microsoft.EntityFrameworkCore;
using IDelivery.Domain.Tenants.Entities;
using IDelivery.Domain.Users.Entities;
using IDelivery.Domain.Catalog.Entities;
using IDelivery.Domain.Common.Entities;

namespace IDelivery.Infrastructure.Persistence.Context;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<AggregateRoot>())
        {
            if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
            {
                // Domain events are handled separately
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}