using IDelivery.Application.Abstractions.CQRS;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.SharedKernel.Common.Result;

namespace IDelivery.Application.Queries.Catalog;

public sealed class GetProductsByCategoryQueryHandler : ICommandHandler<GetProductsByCategoryQuery, IReadOnlyList<ProductResponse>>
{
    private readonly IProductRepository _productRepository;

    public GetProductsByCategoryQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<IReadOnlyList<ProductResponse>>> Handle(GetProductsByCategoryQuery query, CancellationToken cancellationToken = default)
    {
        var products = await _productRepository.GetByCategoryIdAsync(query.CategoryId, cancellationToken);

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
