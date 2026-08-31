using IDelivery.Domain.Common.ValueObjects;
using IDelivery.Domain.Tenants.Enums;
using IDelivery.Domain.Tenants.ValueObjects;

namespace IDelivery.Application.Queries.Tenants;

public sealed record TenantResponse(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    string? LogoUrl,
    TenantStatus Status,
    Address? Address,
    Email? Email,
    PhoneNumber? Phone,
    PhoneNumber? WhatsApp,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);