using IDelivery.Application.Abstractions.CQRS;

namespace IDelivery.Application.Queries.Catalog;

public sealed record GetProductQuery(Guid Id) : IQuery<ProductResponse>;
