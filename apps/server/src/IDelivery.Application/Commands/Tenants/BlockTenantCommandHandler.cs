using IDelivery.Application.Abstractions.Persistence;
using IDelivery.Application.Abstractions.CQRS;
using IDelivery.SharedKernel.Common.Result;
using IDelivery.Application.Commands.Tenants;

namespace IDelivery.Application.Commands.Tenants;

public sealed class BlockTenantCommandHandler : ICommandHandler<BlockTenantCommand>
{
    private readonly ITenantRepository _tenantRepository;

    public BlockTenantCommandHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    public async Task<Result> Handle(BlockTenantCommand command, CancellationToken cancellationToken = default)
    {
        var tenant = await _tenantRepository.GetByIdAsync(command.Id, cancellationToken);
        if (tenant is null)
        {
            return Result.Failure(new Error("Tenant.NotFound", "Tenant não encontrado."));
        }

        var result = tenant.Block();
        if (result.IsFailure)
            return result;

        return Result.Success();
    }
}