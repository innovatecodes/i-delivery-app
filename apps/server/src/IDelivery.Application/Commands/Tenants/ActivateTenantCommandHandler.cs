using IDelivery.Application.Abstractions.Persistence;
using IDelivery.Application.Abstractions.CQRS;
using IDelivery.SharedKernel.Common.Result;
using IDelivery.Application.Commands.Tenants;

namespace IDelivery.Application.Commands.Tenants;

public sealed class ActivateTenantCommandHandler : ICommandHandler<ActivateTenantCommand>
{
    private readonly ITenantRepository _tenantRepository;

    public ActivateTenantCommandHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    public async Task<Result> Handle(ActivateTenantCommand command, CancellationToken cancellationToken = default)
    {
        var tenant = await _tenantRepository.GetByIdAsync(command.Id, cancellationToken);
        if (tenant is null)
        {
            return Result.Failure(new Error("Tenant.NotFound", "Tenant não encontrado."));
        }

        var result = tenant.Activate();
        if (result.IsFailure)
            return result;

        return Result.Success();
    }
}