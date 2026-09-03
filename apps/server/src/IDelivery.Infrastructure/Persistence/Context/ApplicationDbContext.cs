using Microsoft.EntityFrameworkCore;
using IDelivery.Domain.Tenants.Entities;
using IDelivery.Domain.Users.Entities;
using IDelivery.Domain.Catalog.Entities;
using IDelivery.Domain.Carts.Entities;
using IDelivery.Domain.Customers.Entities;
using IDelivery.Domain.Delivery.Entities;
using IDelivery.Domain.Orders.Entities;
using IDelivery.Domain.Payments.Entities;
using IDelivery.Domain.Common.DomainEvents;
using IDelivery.Domain.Common.Entities;
using IDelivery.Application.Abstractions.Events;

namespace IDelivery.Infrastructure.Persistence.Context;

public class ApplicationDbContext : DbContext
{
    private readonly IDomainEventDispatcher? _eventDispatcher;
    private bool _isDispatching;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        IDomainEventDispatcher? eventDispatcher = null) : base(options)
    {
        _eventDispatcher = eventDispatcher;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerAddress> CustomerAddresses => Set<CustomerAddress>();
    public DbSet<DeliverySettings> DeliverySettings => Set<DeliverySettings>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (_isDispatching)
        {
            return await base.SaveChangesAsync(cancellationToken);
        }

        var aggregatesWithEvents = CollectAggregatesWithEvents();

        var result = await base.SaveChangesAsync(cancellationToken);

        if (aggregatesWithEvents.Count > 0 && _eventDispatcher is not null)
        {
            await DispatchDomainEventsAsync(aggregatesWithEvents, cancellationToken);
            ClearDomainEvents(aggregatesWithEvents);
        }

        return result;
    }

    private List<(AggregateRoot Aggregate, List<IDomainEvent> Events)> CollectAggregatesWithEvents()
    {
        var aggregatesWithEvents = new List<(AggregateRoot, List<IDomainEvent>)>();

        foreach (var entry in ChangeTracker.Entries<AggregateRoot>())
        {
            if (entry.Entity.DomainEvents.Count > 0)
            {
                aggregatesWithEvents.Add((entry.Entity, entry.Entity.DomainEvents.ToList()));
            }
        }

        return aggregatesWithEvents;
    }

    private async Task DispatchDomainEventsAsync(
        List<(AggregateRoot Aggregate, List<IDomainEvent> Events)> aggregatesWithEvents,
        CancellationToken cancellationToken)
    {
        _isDispatching = true;

        try
        {
            foreach (var (_, events) in aggregatesWithEvents)
            {
                foreach (var domainEvent in events)
                {
                    await _eventDispatcher!.DispatchAsync(domainEvent, cancellationToken);
                }
            }
        }
        finally
        {
            _isDispatching = false;
        }
    }

    private static void ClearDomainEvents(
        List<(AggregateRoot Aggregate, List<IDomainEvent> Events)> aggregatesWithEvents)
    {
        foreach (var (aggregate, _) in aggregatesWithEvents)
        {
            aggregate.ClearDomainEvents();
        }
    }
}
