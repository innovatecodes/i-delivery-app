namespace IDelivery.Domain.Common.DomainEvents;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
    Guid EventId { get; }
}

public abstract class DomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public Guid EventId { get; } = Guid.NewGuid();

    protected DomainEvent() { }
}