using IDelivery.Application.Abstractions.CQRS;

namespace IDelivery.Application.Commands.Tenants;

public sealed record ActivateTenantCommand(Guid Id) : ICommand;