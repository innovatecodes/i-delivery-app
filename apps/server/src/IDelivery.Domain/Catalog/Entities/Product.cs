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
        decimal price,
        string currency,
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

        if (price < 0)
            return Result.Failure<Product>(new Error("Product.PriceNegative", "Preço não pode ser negativo"));

        if (string.IsNullOrWhiteSpace(currency))
            return Result.Failure<Product>(new Error("Product.CurrencyRequired", "Moeda é obrigatória"));

        Money money;
        try
        {
            money = Money.Create(price, currency);
        }
        catch (Exception ex)
        {
            return Result.Failure<Product>(new Error("Product.InvalidMoney", ex.Message));
        }

        var product = new Product(Guid.NewGuid(), tenantId, categoryId, name.Trim(), description, money, imageUrl, sortOrder);
        return Result.Success(product);
    }

    public Result UpdateDetails(
        string name,
        string? description,
        decimal price,
        string currency,
        Guid? categoryId,
        string? imageUrl,
        int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(new Error("Product.NameRequired", "Nome é obrigatório"));

        if (name.Length > 200)
            return Result.Failure(new Error("Product.NameTooLong", "Nome deve ter no máximo 200 caracteres"));

        if (price < 0)
            return Result.Failure(new Error("Product.PriceNegative", "Preço não pode ser negativo"));

        Money money;
        try
        {
            money = Money.Create(price, currency);
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("Product.InvalidMoney", ex.Message));
        }

        Name = name.Trim();
        Description = description;
        Price = money;
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
