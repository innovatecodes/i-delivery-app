using IDelivery.Application.Abstractions.CQRS;

namespace IDelivery.Application.Queries.Catalog;

public sealed record GetCategoriesByTenantQuery() : IQuery<IReadOnlyList<CategoryResponse>>;
