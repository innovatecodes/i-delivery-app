using IDelivery.Application.Abstractions.CQRS;

namespace IDelivery.Application.Queries.Catalog;

public sealed record GetProductsByCategoryQuery(Guid CategoryId) : IQuery<IReadOnlyList<ProductResponse>>;
