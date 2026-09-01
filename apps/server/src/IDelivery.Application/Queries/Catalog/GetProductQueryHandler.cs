using IDelivery.Application.Abstractions.CQRS;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.Application.Queries.Catalog;
using IDelivery.SharedKernel.Common.Result;

namespace IDelivery.Application.Queries.Catalog;

public sealed class GetProductQueryHandler : ICommandHandler<GetProductQuery, ProductResponse>
{
    private readonly IProductRepository _productRepository;

    public GetProductQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<ProductResponse>> Handle(GetProductQuery query, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(query.Id, cancellationToken);
        if (product is null)
            return Result.Failure<ProductResponse>(new Error("Product.NotFound", "Produto não encontrado"));

        var response = new ProductResponse(
            product.Id,
            product.TenantId,
            product.CategoryId,
            product.Name,
            product.Description,
            product.Price.Amount,
            product.Price.Currency,
            product.ImageUrl,
            product.IsActive,
            product.IsAvailable,
            product.SortOrder,
            product.CreatedAt);

        return Result.Success(response);
    }
}
