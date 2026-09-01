using IDelivery.Domain.Delivery.Enums;

namespace IDelivery.Application.Queries.Delivery;

public sealed record DeliverySettingsResponse(
    Guid Id,
    Guid TenantId,
    DeliveryFeeType FeeType,
    decimal FixedFee,
    decimal? FreeAboveAmount,
    decimal? FeePerKm,
    decimal? MinimumFee,
    decimal? MaximumFee,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);