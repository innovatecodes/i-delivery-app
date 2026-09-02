using IDelivery.Application.Abstractions.CQRS;

namespace IDelivery.Application.Queries.Catalog;

public sealed record GetProductsByTenantQuery() : IQuery<IReadOnlyList<ProductResponse>>;
