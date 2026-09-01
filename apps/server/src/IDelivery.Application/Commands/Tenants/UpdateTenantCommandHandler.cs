using IDelivery.Application.Abstractions.Persistence;
using IDelivery.Application.Abstractions.CQRS;
using IDelivery.SharedKernel.Common.Result;
using IDelivery.Application.Commands.Tenants;

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

        if (command.Address is not null)
        {
            var addressResult = tenant.UpdateAddress(command.Address);
            if (addressResult.IsFailure)
                return addressResult;
        }

        if (command.Email is not null || command.Phone is not null || command.WhatsApp is not null)
        {
            var email = command.Email ?? tenant.Email;
            var phone = command.Phone ?? tenant.Phone;
            var whatsApp = command.WhatsApp ?? tenant.WhatsApp;

            if (email is not null && phone is not null)
            {
                var contactResult = tenant.UpdateContactInfo(email, phone, whatsApp);
                if (contactResult.IsFailure)
                    return contactResult;
            }
        }

        return Result.Success();
    }
}