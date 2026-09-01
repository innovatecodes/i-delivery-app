using IDelivery.Application.Abstractions.CQRS;

namespace IDelivery.Application.Queries.Delivery;

public sealed record GetDeliverySettingsQuery : IQuery<DeliverySettingsResponse>;