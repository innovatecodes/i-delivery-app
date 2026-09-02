using IDelivery.Application.Abstractions.Persistence;
using IDelivery.Domain.Common.ValueObjects;
using IDelivery.SharedKernel.Common.Result;
using IDelivery.Domain.Tenants.Entities;
using IDelivery.Application.Abstractions.CQRS;

namespace IDelivery.Application.Commands.Tenants;

public sealed class CreateTenantCommandHandler : ICommandHandler<CreateTenantCommand, Guid>
{
    private readonly ITenantRepository _tenantRepository;

    public CreateTenantCommandHandler(
        ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    public async Task<Result<Guid>> Handle(CreateTenantCommand command, CancellationToken cancellationToken = default)
    {
        if (await _tenantRepository.ExistsBySlugAsync(command.Slug, cancellationToken))
        {
            return Result.Failure<Guid>(new Error("Tenant.SlugAlreadyExists", "Slug já está em uso"));
        }

        Address? address = null;
        if (!string.IsNullOrWhiteSpace(command.AddressStreet) &&
            !string.IsNullOrWhiteSpace(command.AddressNumber) &&
            !string.IsNullOrWhiteSpace(command.AddressNeighborhood) &&
            !string.IsNullOrWhiteSpace(command.AddressCity) &&
            !string.IsNullOrWhiteSpace(command.AddressState) &&
            !string.IsNullOrWhiteSpace(command.AddressZipCode))
        {
            var addressResult = Address.Create(
                command.AddressStreet,
                command.AddressNumber,
                command.AddressComplement,
                command.AddressNeighborhood,
                command.AddressCity,
                command.AddressState,
                command.AddressZipCode,
                command.AddressReference);
            if (addressResult.IsFailure)
                return Result.Failure<Guid>(addressResult.Error);
            address = addressResult.Value;
        }

        Email? email = null;
        if (!string.IsNullOrWhiteSpace(command.Email))
        {
            var emailResult = Email.Create(command.Email);
            if (emailResult.IsFailure)
                return Result.Failure<Guid>(emailResult.Error);
            email = emailResult.Value;
        }

        PhoneNumber? phone = null;
        if (!string.IsNullOrWhiteSpace(command.Phone))
        {
            var phoneResult = PhoneNumber.Create(command.Phone);
            if (phoneResult.IsFailure)
                return Result.Failure<Guid>(phoneResult.Error);
            phone = phoneResult.Value;
        }

        PhoneNumber? whatsApp = null;
        if (!string.IsNullOrWhiteSpace(command.WhatsApp))
        {
            var whatsAppResult = PhoneNumber.Create(command.WhatsApp);
            if (whatsAppResult.IsFailure)
                return Result.Failure<Guid>(whatsAppResult.Error);
            whatsApp = whatsAppResult.Value;
        }

        var tenantResult = Tenant.Create(
            command.Name,
            command.Slug,
            command.Description,
            command.LogoUrl,
            address,
            email,
            phone,
            whatsApp);

        if (tenantResult.IsFailure)
        {
            return Result.Failure<Guid>(tenantResult.Error);
        }

        await _tenantRepository.AddAsync(tenantResult.Value, cancellationToken);

        return Result.Success(tenantResult.Value.Id);
    }
}
