using IDelivery.Application.Abstractions.CQRS;

namespace IDelivery.Application.Queries.Catalog;

public sealed record GetCategoryQuery(Guid Id) : IQuery<CategoryResponse>;
