using IDelivery.Application.Abstractions.CQRS;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.SharedKernel.Common.Result;

namespace IDelivery.Application.Commands.Catalog;

public sealed class UpdateCategoryCommandHandler : ICommandHandler<UpdateCategoryCommand>
{
    private readonly ICategoryRepository _categoryRepository;

    public UpdateCategoryCommandHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Result> Handle(UpdateCategoryCommand command, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(command.Id, cancellationToken);
        if (category is null)
            return Result.Failure(new Error("Category.NotFound", "Categoria não encontrada"));

        if (await _categoryRepository.ExistsByNameAsync(category.TenantId, command.Name, command.Id, cancellationToken))
            return Result.Failure(new Error("Category.NameAlreadyExists", "Já existe uma categoria com este nome"));

        var updateResult = category.UpdateDetails(
            command.Name,
            command.Description,
            command.ImageUrl,
            command.SortOrder);

        if (updateResult.IsFailure)
            return Result.Failure(updateResult.Error);

        await _categoryRepository.UpdateAsync(category, cancellationToken);

        return Result.Success();
    }
}
