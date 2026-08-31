using IDelivery.Domain.Common.ValueObjects;
using IDelivery.Domain.Tenants.ValueObjects;
using IDelivery.Application.Abstractions.CQRS;

namespace IDelivery.Application.Commands.Tenants;

public sealed record CreateTenantCommand(
    string Name,
    string Slug,
    string? Description = null,
    string? LogoUrl = null,
    Address? Address = null,
    Email? Email = null,
    PhoneNumber? Phone = null,
    PhoneNumber? WhatsApp = null
) : ICommand<Guid>;