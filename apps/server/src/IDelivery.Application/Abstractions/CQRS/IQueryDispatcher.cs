using IDelivery.SharedKernel.Common.Result;

namespace IDelivery.Application.Abstractions.CQRS
{
    public interface IQueryDispatcher
    {
        Task<Result<TResult>> Dispatch<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default) where TQuery : IQuery<TResult>;
    }
}
