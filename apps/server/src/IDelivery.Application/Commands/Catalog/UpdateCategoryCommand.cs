using IDelivery.Application.Abstractions.CQRS;

namespace IDelivery.Application.Commands.Catalog;

public sealed record UpdateCategoryCommand(
    Guid Id,
    string Name,
    string? Description,
    string? ImageUrl,
    int SortOrder) : ICommand;
