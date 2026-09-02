using IDelivery.SharedKernel.Common.Result;
using IDelivery.Domain.Common.Entities;
using IDelivery.Domain.Common.ValueObjects;
using IDelivery.Domain.Catalog.Events;

namespace IDelivery.Domain.Catalog.Entities;

public sealed class Product : AggregateRoot
{
    public Guid TenantId { get; private set; }
    public Guid? CategoryId { get; private set; }
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public Money Price { get; private set; } = null!;
    public string? ImageUrl { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsAvailable { get; private set; }
    public int SortOrder { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Product() { }

    private Product(
        Guid id,
        Guid tenantId,
        Guid? categoryId,
        string name,
        string? description,
        Money price,
        string? imageUrl,
        int sortOrder) : base(id)
    {
        TenantId = tenantId;
        CategoryId = categoryId;
        Name = name;
        Description = description;
        Price = price;
        ImageUrl = imageUrl;
        SortOrder = sortOrder;
        IsActive = true;
        IsAvailable = true;
        CreatedAt = DateTime.UtcNow;

        AddDomainEvent(new ProductCreatedDomainEvent(id, tenantId, name));
    }

    public static Result<Product> Create(
        Guid tenantId,
        string name,
        Money price,
        Guid? categoryId = null,
        string? description = null,
        string? imageUrl = null,
        int sortOrder = 0)
    {
        if (tenantId == Guid.Empty)
            return Result.Failure<Product>(new Error("Product.TenantRequired", "Tenant é obrigatório"));

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Product>(new Error("Product.NameRequired", "Nome é obrigatório"));

        if (name.Length > 200)
            return Result.Failure<Product>(new Error("Product.NameTooLong", "Nome deve ter no máximo 200 caracteres"));

        if (price is null)
            return Result.Failure<Product>(new Error("Product.PriceRequired", "Preço é obrigatório"));

        var product = new Product(Guid.NewGuid(), tenantId, categoryId, name.Trim(), description, price, imageUrl, sortOrder);
        return Result.Success(product);
    }

    public Result UpdateDetails(
        string name,
        string? description,
        Money price,
        Guid? categoryId,
        string? imageUrl,
        int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(new Error("Product.NameRequired", "Nome é obrigatório"));

        if (name.Length > 200)
            return Result.Failure(new Error("Product.NameTooLong", "Nome deve ter no máximo 200 caracteres"));

        if (price is null)
            return Result.Failure(new Error("Product.PriceRequired", "Preço é obrigatório"));

        Name = name.Trim();
        Description = description;
        Price = price;
        CategoryId = categoryId;
        ImageUrl = imageUrl;
        SortOrder = sortOrder;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ProductUpdatedDomainEvent(Id, TenantId, Name));

        return Result.Success();
    }

    public Result Activate()
    {
        if (IsActive)
            return Result.Failure(new Error("Product.AlreadyActive", "Produto já está ativo"));

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result Deactivate()
    {
        if (!IsActive)
            return Result.Failure(new Error("Product.AlreadyInactive", "Produto já está inativo"));

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result MarkAsAvailable()
    {
        if (IsAvailable)
            return Result.Failure(new Error("Product.AlreadyAvailable", "Produto já está disponível"));

        IsAvailable = true;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result MarkAsUnavailable()
    {
        if (!IsAvailable)
            return Result.Failure(new Error("Product.AlreadyUnavailable", "Produto já está indisponível"));

        IsAvailable = false;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result ChangeCategory(Guid? categoryId)
    {
        CategoryId = categoryId;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }
}
