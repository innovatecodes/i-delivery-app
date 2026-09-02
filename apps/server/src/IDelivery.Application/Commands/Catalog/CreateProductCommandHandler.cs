using IDelivery.Application.Abstractions.Authentication;
using IDelivery.Application.Abstractions.CQRS;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.Domain.Catalog.Entities;
using IDelivery.Domain.Common.ValueObjects;
using IDelivery.SharedKernel.Common.Result;

namespace IDelivery.Application.Commands.Catalog;

public sealed class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, Guid>
{
    private readonly IProductRepository _productRepository;
    private readonly ITenantContext _tenantContext;

    public CreateProductCommandHandler(
        IProductRepository productRepository,
        ITenantContext tenantContext)
    {
        _productRepository = productRepository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<Guid>> Handle(CreateProductCommand command, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantContext.TenantId;
        if (!tenantId.HasValue)
            return Result.Failure<Guid>(new Error("Product.TenantRequired", "Tenant é obrigatório"));

        if (await _productRepository.ExistsByNameAsync(tenantId.Value, command.Name, cancellationToken: cancellationToken))
            return Result.Failure<Guid>(new Error("Product.NameAlreadyExists", "Já existe um produto com este nome"));

        var priceResult = Money.Create(command.Price, command.Currency);
        if (priceResult.IsFailure)
            return Result.Failure<Guid>(priceResult.Error);

        var price = priceResult.Value;

        var productResult = Product.Create(
            tenantId.Value,
            command.Name,
            price,
            command.CategoryId,
            command.Description,
            command.ImageUrl,
            command.SortOrder);

        if (productResult.IsFailure)
            return Result.Failure<Guid>(productResult.Error);

        var product = productResult.Value;
        await _productRepository.AddAsync(product, cancellationToken);

        return Result.Success(product.Id);
    }
}
