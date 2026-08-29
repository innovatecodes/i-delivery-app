using Microsoft.Extensions.DependencyInjection;
using IDelivery.Application.CQRS;
using IDelivery.Domain.Common.Result;

namespace IDelivery.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICommandDispatcher, CommandDispatcher>();
        services.AddScoped<IQueryDispatcher, QueryDispatcher>();
        return services;
    }
}

public interface ICommandDispatcher
{
    Task<Result> Dispatch<TCommand>(TCommand command, CancellationToken cancellationToken = default) where TCommand : ICommand;
    Task<Result<TResult>> Dispatch<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default) where TCommand : ICommand<TResult>;
}

public interface IQueryDispatcher
{
    Task<Result<TResult>> Dispatch<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default) where TQuery : IQuery<TResult>;
}

internal sealed class CommandDispatcher : ICommandDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public CommandDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<Result> Dispatch<TCommand>(TCommand command, CancellationToken cancellationToken = default) where TCommand : ICommand
    {
        var handler = _serviceProvider.GetRequiredService<ICommandHandler<TCommand>>();
        return await handler.Handle(command, cancellationToken);
    }

    public async Task<Result<TResult>> Dispatch<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default) where TCommand : ICommand<TResult>
    {
        var handler = _serviceProvider.GetRequiredService<ICommandHandler<TCommand, TResult>>();
        return await handler.Handle(command, cancellationToken);
    }
}

internal sealed class QueryDispatcher : IQueryDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public QueryDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<Result<TResult>> Dispatch<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default) where TQuery : IQuery<TResult>
    {
        var handler = _serviceProvider.GetRequiredService<IQueryHandler<TQuery, TResult>>();
        return await handler.Handle(query, cancellationToken);
    }
}