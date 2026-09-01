using IDelivery.Application.Abstractions.Authentication;
using IDelivery.Application.Abstractions.CQRS;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.SharedKernel.Common.Result;

namespace IDelivery.Application.Commands.Delivery;

public sealed class UpdateDeliverySettingsCommandHandler : ICommandHandler<UpdateDeliverySettingsCommand>
{
    private readonly IDeliverySettingsRepository _deliverySettingsRepository;
    private readonly ITenantContext _tenantContext;

    public UpdateDeliverySettingsCommandHandler(
        IDeliverySettingsRepository deliverySettingsRepository,
        ITenantContext tenantContext)
    {
        _deliverySettingsRepository = deliverySettingsRepository;
        _tenantContext = tenantContext;
    }

    public async Task<Result> Handle(UpdateDeliverySettingsCommand command, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantContext.TenantId;
        if (!tenantId.HasValue)
            return Result.Failure(new Error("DeliverySettings.TenantRequired", "Tenant é obrigatório"));

        var settings = await _deliverySettingsRepository.GetByTenantIdAsync(tenantId.Value, cancellationToken);
        if (settings is null)
            return Result.Failure(new Error("DeliverySettings.NotFound", "Configurações de entrega não encontradas"));

        var updateResult = settings.Update(
            command.FeeType,
            command.FixedFee,
            command.FreeAboveAmount,
            command.FeePerKm,
            command.MinimumFee,
            command.MaximumFee);

        if (updateResult.IsFailure)
            return Result.Failure(updateResult.Error);

        await _deliverySettingsRepository.UpdateAsync(settings, cancellationToken);

        return Result.Success();
    }
}