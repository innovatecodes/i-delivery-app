using IDelivery.Domain.Common.DomainEvents;

namespace IDelivery.Application.Abstractions.Events;

public interface IDomainEventHandler<in TDomainEvent> where TDomainEvent : IDomainEvent
{
    Task Handle(TDomainEvent domainEvent, CancellationToken cancellationToken = default);
}
