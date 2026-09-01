using IDelivery.SharedKernel.Common.Result;
using IDelivery.Domain.Common.Entities;
using IDelivery.Domain.Catalog.Events;

namespace IDelivery.Domain.Catalog.Entities;

public sealed class Category : AggregateRoot
{
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public string? ImageUrl { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Category() { }

    private Category(
        Guid id,
        Guid tenantId,
        string name,
        string? description,
        string? imageUrl,
        int sortOrder) : base(id)
    {
        TenantId = tenantId;
        Name = name;
        Description = description;
        ImageUrl = imageUrl;
        SortOrder = sortOrder;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;

        AddDomainEvent(new CategoryCreatedDomainEvent(id, tenantId, name));
    }

    public static Result<Category> Create(
        Guid tenantId,
        string name,
        string? description = null,
        string? imageUrl = null,
        int sortOrder = 0)
    {
        if (tenantId == Guid.Empty)
            return Result.Failure<Category>(new Error("Category.TenantRequired", "Tenant é obrigatório"));

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Category>(new Error("Category.NameRequired", "Nome é obrigatório"));

        if (name.Length > 100)
            return Result.Failure<Category>(new Error("Category.NameTooLong", "Nome deve ter no máximo 100 caracteres"));

        var category = new Category(Guid.NewGuid(), tenantId, name.Trim(), description, imageUrl, sortOrder);
        return Result.Success(category);
    }

    public Result UpdateDetails(string name, string? description, string? imageUrl, int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(new Error("Category.NameRequired", "Nome é obrigatório"));

        if (name.Length > 100)
            return Result.Failure(new Error("Category.NameTooLong", "Nome deve ter no máximo 100 caracteres"));

        Name = name.Trim();
        Description = description;
        ImageUrl = imageUrl;
        SortOrder = sortOrder;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new CategoryUpdatedDomainEvent(Id, TenantId, Name));

        return Result.Success();
    }

    public Result Activate()
    {
        if (IsActive)
            return Result.Failure(new Error("Category.AlreadyActive", "Categoria já está ativa"));

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result Deactivate()
    {
        if (!IsActive)
            return Result.Failure(new Error("Category.AlreadyInactive", "Categoria já está inativa"));

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }
}
