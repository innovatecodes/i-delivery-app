namespace IDelivery.Application.Queries.Catalog;

public sealed record CategoryResponse(
    Guid Id,
    Guid TenantId,
    string Name,
    string? Description,
    string? ImageUrl,
    int SortOrder,
    bool IsActive,
    DateTime CreatedAt);
