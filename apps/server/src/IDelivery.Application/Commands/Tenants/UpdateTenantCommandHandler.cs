using IDelivery.Application.Abstractions.Persistence;
using IDelivery.Domain.Common.ValueObjects;
using IDelivery.SharedKernel.Common.Result;
using IDelivery.Application.Abstractions.CQRS;

namespace IDelivery.Application.Commands.Tenants;

public sealed class UpdateTenantCommandHandler : ICommandHandler<UpdateTenantCommand>
{
    private readonly ITenantRepository _tenantRepository;

    public UpdateTenantCommandHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    public async Task<Result> Handle(UpdateTenantCommand command, CancellationToken cancellationToken = default)
    {
        var tenant = await _tenantRepository.GetByIdAsync(command.Id, cancellationToken);
        if (tenant is null)
        {
            return Result.Failure(new Error("Tenant.NotFound", "Tenant não encontrado"));
        }

        var result = tenant.UpdateDetails(command.Name, command.Description, command.LogoUrl);
        if (result.IsFailure)
            return result;

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
                return Result.Failure(addressResult.Error);

            var updateAddressResult = tenant.UpdateAddress(addressResult.Value);
            if (updateAddressResult.IsFailure)
                return updateAddressResult;
        }

        if (!string.IsNullOrWhiteSpace(command.Email) || !string.IsNullOrWhiteSpace(command.Phone))
        {
            Email? email = null;
            PhoneNumber? phone = null;
            PhoneNumber? whatsApp = null;

            if (!string.IsNullOrWhiteSpace(command.Email))
            {
                var emailResult = Email.Create(command.Email);
                if (emailResult.IsFailure)
                    return Result.Failure(emailResult.Error);
                email = emailResult.Value;
            }

            if (!string.IsNullOrWhiteSpace(command.Phone))
            {
                var phoneResult = PhoneNumber.Create(command.Phone);
                if (phoneResult.IsFailure)
                    return Result.Failure(phoneResult.Error);
                phone = phoneResult.Value;
            }

            if (!string.IsNullOrWhiteSpace(command.WhatsApp))
            {
                var whatsAppResult = PhoneNumber.Create(command.WhatsApp);
                if (whatsAppResult.IsFailure)
                    return Result.Failure(whatsAppResult.Error);
                whatsApp = whatsAppResult.Value;
            }

            var finalEmail = email ?? tenant.Email;
            var finalPhone = phone ?? tenant.Phone;

            if (finalEmail is not null && finalPhone is not null)
            {
                var contactResult = tenant.UpdateContactInfo(finalEmail, finalPhone, whatsApp);
                if (contactResult.IsFailure)
                    return contactResult;
            }
        }

        return Result.Success();
    }
}
