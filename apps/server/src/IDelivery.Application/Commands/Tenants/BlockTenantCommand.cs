using IDelivery.Application.Abstractions.CQRS;

namespace IDelivery.Application.Commands.Tenants;

public sealed record BlockTenantCommand(Guid Id) : ICommand;