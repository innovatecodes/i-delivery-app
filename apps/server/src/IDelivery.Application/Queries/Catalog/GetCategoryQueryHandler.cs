using IDelivery.Application.Abstractions.CQRS;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.Application.Queries.Catalog;
using IDelivery.SharedKernel.Common.Result;

namespace IDelivery.Application.Queries.Catalog;

public sealed class GetCategoryQueryHandler : IQueryHandler<GetCategoryQuery, CategoryResponse>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetCategoryQueryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<CategoryResponse>> Handle(GetCategoryQuery query, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(query.Id, cancellationToken);
        if (category is null)
            return Result.Failure<CategoryResponse>(new Error("Category.NotFound", "Categoria não encontrada"));

        var response = new CategoryResponse(
            category.Id,
            category.TenantId,
            category.Name,
            category.Description,
            category.ImageUrl,
            category.SortOrder,
            category.IsActive,
            category.CreatedAt);

        return Result.Success(response);
    }
}
