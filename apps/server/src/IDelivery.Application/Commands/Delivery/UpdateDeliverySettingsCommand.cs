using IDelivery.Application.Abstractions.CQRS;
using IDelivery.Domain.Delivery.Enums;

namespace IDelivery.Application.Commands.Delivery;

public sealed record UpdateDeliverySettingsCommand(
    DeliveryFeeType FeeType,
    decimal FixedFee,
    decimal? FreeAboveAmount,
    decimal? FeePerKm,
    decimal? MinimumFee,
    decimal? MaximumFee) : ICommand;