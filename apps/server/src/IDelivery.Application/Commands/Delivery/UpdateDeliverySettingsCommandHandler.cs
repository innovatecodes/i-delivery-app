using IDelivery.Application.Abstractions.Authentication;
using IDelivery.Application.Abstractions.CQRS;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.Domain.Common.ValueObjects;
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

        var fixedFeeResult = Money.Create(command.FixedFee);
        if (fixedFeeResult.IsFailure)
            return Result.Failure(fixedFeeResult.Error);

        var fixedFee = fixedFeeResult.Value;

        Money? freeAboveAmount = null;
        if (command.FreeAboveAmount.HasValue)
        {
            var freeAboveAmountResult = Money.Create(command.FreeAboveAmount.Value);
            if (freeAboveAmountResult.IsFailure)
                return Result.Failure(freeAboveAmountResult.Error);
            freeAboveAmount = freeAboveAmountResult.Value;
        }

        Money? feePerKm = null;
        if (command.FeePerKm.HasValue)
        {
            var feePerKmResult = Money.Create(command.FeePerKm.Value);
            if (feePerKmResult.IsFailure)
                return Result.Failure(feePerKmResult.Error);
            feePerKm = feePerKmResult.Value;
        }

        Money? minimumFee = null;
        if (command.MinimumFee.HasValue)
        {
            var minimumFeeResult = Money.Create(command.MinimumFee.Value);
            if (minimumFeeResult.IsFailure)
                return Result.Failure(minimumFeeResult.Error);
            minimumFee = minimumFeeResult.Value;
        }

        Money? maximumFee = null;
        if (command.MaximumFee.HasValue)
        {
            var maximumFeeResult = Money.Create(command.MaximumFee.Value);
            if (maximumFeeResult.IsFailure)
                return Result.Failure(maximumFeeResult.Error);
            maximumFee = maximumFeeResult.Value;
        }

        var updateResult = settings.Update(
            command.FeeType,
            fixedFee,
            freeAboveAmount,
            feePerKm,
            minimumFee,
            maximumFee);

        if (updateResult.IsFailure)
            return Result.Failure(updateResult.Error);

        await _deliverySettingsRepository.UpdateAsync(settings, cancellationToken);

        return Result.Success();
    }
}
