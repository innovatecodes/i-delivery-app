using IDelivery.Application.Abstractions.Persistence;
using IDelivery.Application.Abstractions.CQRS;
using IDelivery.SharedKernel.Common.Result;
using IDelivery.Application.Commands.Tenants;

namespace IDelivery.Application.Commands.Tenants;

public sealed class DeleteTenantCommandHandler : ICommandHandler<DeleteTenantCommand>
{
    private readonly ITenantRepository _tenantRepository;

    public DeleteTenantCommandHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    public async Task<Result> Handle(DeleteTenantCommand command, CancellationToken cancellationToken = default)
    {
        var tenant = await _tenantRepository.GetByIdAsync(command.Id, cancellationToken);
        if (tenant is null)
        {
            return Result.Failure(new Error("Tenant.NotFound", "Tenant não encontrado"));
        }

        // Usa Block() como soft delete (marca como bloqueado)
        var result = tenant.Block();
        if (result.IsFailure)
            return result;

        return Result.Success();
    }
}