using IDelivery.Application.Abstractions.CQRS;

namespace IDelivery.Application.Queries.Carts;

public sealed record GetCartQuery : IQuery<CartResponse>;
