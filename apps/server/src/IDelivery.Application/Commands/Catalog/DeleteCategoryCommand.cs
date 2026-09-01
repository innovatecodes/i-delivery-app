using IDelivery.Application.Abstractions.CQRS;

namespace IDelivery.Application.Commands.Catalog;

public sealed record DeleteCategoryCommand(Guid Id) : ICommand;
