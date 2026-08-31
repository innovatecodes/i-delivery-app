using IDelivery.Application.Abstractions.CQRS;
using IDelivery.Domain.Common.ValueObjects;
using IDelivery.Domain.Tenants.ValueObjects;

namespace IDelivery.Application.Commands.Tenants;

public sealed record UpdateTenantCommand(
    Guid Id,
    string Name,
    string? Description = null,
    string? LogoUrl = null,
    Address? Address = null,
    Email? Email = null,
    PhoneNumber? Phone = null,
    PhoneNumber? WhatsApp = null
) : ICommand;