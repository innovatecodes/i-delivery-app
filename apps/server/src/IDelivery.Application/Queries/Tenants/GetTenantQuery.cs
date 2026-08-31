using IDelivery.Application.Abstractions.CQRS;

namespace IDelivery.Application.Queries.Tenants;

public sealed record GetTenantQuery(Guid Id) : IQuery<TenantResponse>;