using IDelivery.Application.Abstractions.CQRS;
using IDelivery.Application.Common.Models;

namespace IDelivery.Application.Queries.Tenants;

public sealed record GetTenantsQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    string? Status = null
) : IQuery<PagedResult<TenantListItemResponse>>;