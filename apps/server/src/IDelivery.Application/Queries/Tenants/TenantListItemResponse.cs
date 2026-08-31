using IDelivery.Domain.Tenants.Enums;

namespace IDelivery.Application.Queries.Tenants;

public sealed record TenantListItemResponse(
    Guid Id,
    string Name,
    string Slug,
    TenantStatus Status,
    DateTime CreatedAt
);