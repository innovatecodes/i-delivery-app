using IDelivery.Application.Abstractions.Persistence;
using IDelivery.Application.Abstractions.CQRS;
using IDelivery.Application.Common.Models;
using IDelivery.Domain.Tenants.Enums;
using IDelivery.SharedKernel.Common.Result;

namespace IDelivery.Application.Queries.Tenants;

public sealed class GetTenantsQueryHandler : IQueryHandler<GetTenantsQuery, PagedResult<TenantListItemResponse>>
{
    private readonly ITenantRepository _tenantRepository;

    public GetTenantsQueryHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    public async Task<Result<PagedResult<TenantListItemResponse>>> Handle(GetTenantsQuery query, CancellationToken cancellationToken = default)
    {
        var status = ParseTenantStatus(query.Status);

        var tenants = await _tenantRepository.GetAllAsync(
            query.Page,
            query.PageSize,
            query.Search,
            status,
            cancellationToken);

        var totalCount = await _tenantRepository.CountAsync(
            query.Search,
            status,
            cancellationToken);

        var items = tenants.Select(t => new TenantListItemResponse(
            t.Id,
            t.Name,
            t.Slug,
            t.Status,
            t.CreatedAt)).ToList();

        var result = new PagedResult<TenantListItemResponse>(
            items,
            totalCount,
            query.Page,
            query.PageSize);

        return Result.Success(result);
    }

    private static TenantStatus? ParseTenantStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return null;

        if (Enum.TryParse<TenantStatus>(status, true, out var parsed))
            return parsed;

        return null;
    }
}