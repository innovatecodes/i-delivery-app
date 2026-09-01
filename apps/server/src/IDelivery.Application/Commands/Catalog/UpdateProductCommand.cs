using IDelivery.Application.Abstractions.CQRS;

namespace IDelivery.Application.Commands.Catalog;

public sealed record UpdateProductCommand(
    Guid Id,
    string Name,
    decimal Price,
    string Currency,
    Guid? CategoryId,
    string? Description,
    string? ImageUrl,
    int SortOrder) : ICommand;
