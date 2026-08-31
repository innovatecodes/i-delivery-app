using IDelivery.SharedKernel.Common.Result;

namespace IDelivery.Application.Abstractions.CQRS;

public interface ICommandDispatcher
{
    Task<Result> Dispatch<TCommand>(TCommand command, CancellationToken cancellationToken = default) where TCommand : ICommand;
    Task<Result<TResult>> Dispatch<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default) where TCommand : ICommand<TResult>;
}
