using IDelivery.Application.Abstractions.CQRS;

namespace IDelivery.Application.Commands.Delivery;

public sealed record DeleteDeliverySettingsCommand : ICommand;