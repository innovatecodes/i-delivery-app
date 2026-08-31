using IDelivery.Application.Abstractions.Persistence;
using IDelivery.Application.Abstractions.CQRS;
using IDelivery.SharedKernel.Common.Result;

namespace IDelivery.Application.Queries.Tenants;

public sealed class GetTenantQueryHandler : IQueryHandler<GetTenantQuery, TenantResponse>
{
    private readonly ITenantRepository _tenantRepository;

    public GetTenantQueryHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    public async Task<Result<TenantResponse>> Handle(GetTenantQuery query, CancellationToken cancellationToken = default)
    {
        var tenant = await _tenantRepository.GetByIdAsync(query.Id, cancellationToken);
        if (tenant is null)
        {
            return Result.Failure<TenantResponse>(new Error("Tenant.NotFound", "Tenant não encontrado."));
        }

        var response = new TenantResponse(
            tenant.Id,
            tenant.Name,
            tenant.Slug,
            tenant.Description,
            tenant.LogoUrl,
            tenant.Status,
            tenant.Address,
            tenant.Email,
            tenant.Phone,
            tenant.WhatsApp,
            tenant.CreatedAt,
            tenant.UpdatedAt);

        return Result.Success(response);
    }
}