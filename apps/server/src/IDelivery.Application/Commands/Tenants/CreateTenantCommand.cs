using IDelivery.Application.Abstractions.CQRS;

namespace IDelivery.Application.Commands.Tenants;

public sealed record CreateTenantCommand(
    string Name,
    string Slug,
    string? Description = null,
    string? LogoUrl = null,
    string? AddressStreet = null,
    string? AddressNumber = null,
    string? AddressComplement = null,
    string? AddressNeighborhood = null,
    string? AddressCity = null,
    string? AddressState = null,
    string? AddressZipCode = null,
    string? AddressReference = null,
    string? Email = null,
    string? Phone = null,
    string? WhatsApp = null
) : ICommand<Guid>;
