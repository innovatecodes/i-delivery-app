using IDelivery.Application.Abstractions.Authentication;
using IDelivery.Application.Abstractions.CQRS;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.SharedKernel.Common.Result;

namespace IDelivery.Application.Queries.Catalog;

public sealed class GetCategoriesByTenantQueryHandler : IQueryHandler<GetCategoriesByTenantQuery, IReadOnlyList<CategoryResponse>>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ITenantContext _tenantContext;

    public GetCategoriesByTenantQueryHandler(
        ICategoryRepository categoryRepository,
        ITenantContext tenantContext)
    {
        _categoryRepository = categoryRepository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<IReadOnlyList<CategoryResponse>>> Handle(GetCategoriesByTenantQuery query, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantContext.TenantId;
        if (!tenantId.HasValue)
            return Result.Failure<IReadOnlyList<CategoryResponse>>(new Error("Category.TenantRequired", "Tenant é obrigatório"));

        var categories = await _categoryRepository.GetByTenantIdAsync(tenantId.Value, cancellationToken);

        var responses = categories.Select(c => new CategoryResponse(
            c.Id,
            c.TenantId,
            c.Name,
            c.Description,
            c.ImageUrl,
            c.SortOrder,
            c.IsActive,
            c.CreatedAt)).ToList();

        return Result.Success<IReadOnlyList<CategoryResponse>>(responses);
    }
}
