using IDelivery.SharedKernel.Common.Result;

namespace IDelivery.Application.Abstractions.CQRS;

public interface IQueryHandler<in TQuery, TResult> where TQuery : IQuery<TResult>
{
    Task<Result<TResult>> Handle(TQuery query, CancellationToken cancellationToken = default);
}