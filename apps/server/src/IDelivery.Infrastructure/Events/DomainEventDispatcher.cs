using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using IDelivery.Application.Abstractions.Events;
using IDelivery.Domain.Common.DomainEvents;

namespace IDelivery.Infrastructure.Events;

public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private static readonly ConcurrentDictionary<Type, Type> HandlerTypeCache = new();

    public DomainEventDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var handlerType = HandlerTypeCache.GetOrAdd(
            domainEvent.GetType(),
            t => typeof(IDomainEventHandler<>).MakeGenericType(t));

        var handlers = _serviceProvider.GetServices(handlerType);

        foreach (var handler in handlers)
        {
            await ((dynamic)handler).Handle((dynamic)domainEvent, cancellationToken);
        }
    }

    public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            await DispatchAsync(domainEvent, cancellationToken);
        }
    }
}
