using IDelivery.Application.Abstractions.CQRS;

namespace IDelivery.Application.Commands.Catalog;

public sealed record DeleteProductCommand(Guid Id) : ICommand;
