using IDelivery.Application.Abstractions.CQRS;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.Domain.Common.ValueObjects;
using IDelivery.SharedKernel.Common.Result;

namespace IDelivery.Application.Commands.Catalog;

public sealed class UpdateProductCommandHandler : ICommandHandler<UpdateProductCommand>
{
    private readonly IProductRepository _productRepository;

    public UpdateProductCommandHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result> Handle(UpdateProductCommand command, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(command.Id, cancellationToken);
        if (product is null)
            return Result.Failure(new Error("Product.NotFound", "Produto não encontrado"));

        if (await _productRepository.ExistsByNameAsync(product.TenantId, command.Name, command.Id, cancellationToken))
            return Result.Failure(new Error("Product.NameAlreadyExists", "Já existe um produto com este nome"));

        var priceResult = Money.Create(command.Price, command.Currency);
        if (priceResult.IsFailure)
            return Result.Failure(priceResult.Error);

        var price = priceResult.Value;

        var updateResult = product.UpdateDetails(
            command.Name,
            command.Description,
            price,
            command.CategoryId,
            command.ImageUrl,
            command.SortOrder);

        if (updateResult.IsFailure)
            return Result.Failure(updateResult.Error);

        await _productRepository.UpdateAsync(product, cancellationToken);

        return Result.Success();
    }
}
