using IDelivery.Application.Abstractions.Authentication;
using IDelivery.Application.Abstractions.CQRS;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.Domain.Catalog.Entities;
using IDelivery.SharedKernel.Common.Result;

namespace IDelivery.Application.Commands.Catalog;

public sealed class CreateCategoryCommandHandler : ICommandHandler<CreateCategoryCommand, Guid>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ITenantContext _tenantContext;

    public CreateCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        ITenantContext tenantContext)
    {
        _categoryRepository = categoryRepository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<Guid>> Handle(CreateCategoryCommand command, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantContext.TenantId;
        if (!tenantId.HasValue)
            return Result.Failure<Guid>(new Error("Category.TenantRequired", "Tenant é obrigatório"));

        if (await _categoryRepository.ExistsByNameAsync(tenantId.Value, command.Name, cancellationToken: cancellationToken))
            return Result.Failure<Guid>(new Error("Category.NameAlreadyExists", "Já existe uma categoria com este nome"));

        var categoryResult = Category.Create(
            tenantId.Value,
            command.Name,
            command.Description,
            command.ImageUrl,
            command.SortOrder);

        if (categoryResult.IsFailure)
            return Result.Failure<Guid>(categoryResult.Error);

        var category = categoryResult.Value;
        await _categoryRepository.AddAsync(category, cancellationToken);

        return Result.Success(category.Id);
    }
}
