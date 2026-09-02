using IDelivery.Application.Abstractions.Authentication;
using IDelivery.Application.Abstractions.CQRS;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.SharedKernel.Common.Result;

namespace IDelivery.Application.Queries.Catalog;

public sealed class GetProductsByTenantQueryHandler : IQueryHandler<GetProductsByTenantQuery, IReadOnlyList<ProductResponse>>
{
    private readonly IProductRepository _productRepository;
    private readonly ITenantContext _tenantContext;

    public GetProductsByTenantQueryHandler(
        IProductRepository productRepository,
        ITenantContext tenantContext)
    {
        _productRepository = productRepository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<IReadOnlyList<ProductResponse>>> Handle(GetProductsByTenantQuery query, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantContext.TenantId;
        if (!tenantId.HasValue)
            return Result.Failure<IReadOnlyList<ProductResponse>>(new Error("Product.TenantRequired", "Tenant é obrigatório"));

        var products = await _productRepository.GetByTenantIdAsync(tenantId.Value, cancellationToken);

        var responses = products.Select(p => new ProductResponse(
            p.Id,
            p.TenantId,
            p.CategoryId,
            p.Name,
            p.Description,
            p.Price.Amount,
            p.Price.Currency,
            p.ImageUrl,
            p.IsActive,
            p.IsAvailable,
            p.SortOrder,
            p.CreatedAt)).ToList();

        return Result.Success<IReadOnlyList<ProductResponse>>(responses);
    }
}
