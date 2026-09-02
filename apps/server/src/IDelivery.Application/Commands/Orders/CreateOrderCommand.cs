using IDelivery.Application.Abstractions.CQRS;

namespace IDelivery.Application.Commands.Orders;

public sealed record CreateOrderCommand(
    IReadOnlyList<CreateOrderItemDto> Items,
    decimal DeliveryFee,
    string Currency,
    string DeliveryStreet,
    string DeliveryNumber,
    string? DeliveryComplement,
    string DeliveryNeighborhood,
    string DeliveryCity,
    string DeliveryState,
    string DeliveryZipCode,
    string? DeliveryReference,
    decimal? DeliveryDistanceKm) : ICommand<Guid>;

public sealed record CreateOrderItemDto(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    string Currency,
    int Quantity);
