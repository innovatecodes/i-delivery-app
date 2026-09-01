namespace IDelivery.Application.Queries.Catalog;

public sealed record ProductResponse(
    Guid Id,
    Guid TenantId,
    Guid? CategoryId,
    string Name,
    string? Description,
    decimal Price,
    string Currency,
    string? ImageUrl,
    bool IsActive,
    bool IsAvailable,
    int SortOrder,
    DateTime CreatedAt);
