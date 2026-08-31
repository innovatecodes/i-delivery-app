namespace IDelivery.Domain.Common.DomainEvents;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
    Guid EventId { get; }
}