using IDelivery.Application.Abstractions.Authentication;
using IDelivery.Application.Abstractions.CQRS;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.Domain.Common.ValueObjects;
using IDelivery.Domain.Delivery.Entities;
using IDelivery.SharedKernel.Common.Result;

namespace IDelivery.Application.Commands.Delivery;

public sealed class CreateDeliverySettingsCommandHandler : ICommandHandler<CreateDeliverySettingsCommand, Guid>
{
    private readonly IDeliverySettingsRepository _deliverySettingsRepository;
    private readonly ITenantContext _tenantContext;

    public CreateDeliverySettingsCommandHandler(
        IDeliverySettingsRepository deliverySettingsRepository,
        ITenantContext tenantContext)
    {
        _deliverySettingsRepository = deliverySettingsRepository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<Guid>> Handle(CreateDeliverySettingsCommand command, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantContext.TenantId;
        if (!tenantId.HasValue)
            return Result.Failure<Guid>(new Error("DeliverySettings.TenantRequired", "Tenant é obrigatório"));

        var existing = await _deliverySettingsRepository.GetByTenantIdAsync(tenantId.Value, cancellationToken);
        if (existing is not null)
            return Result.Failure<Guid>(new Error("DeliverySettings.AlreadyExists", "Configurações de entrega já existem para este tenant"));

        var fixedFeeResult = Money.Create(command.FixedFee);
        if (fixedFeeResult.IsFailure)
            return Result.Failure<Guid>(fixedFeeResult.Error);

        var fixedFee = fixedFeeResult.Value;

        Money? freeAboveAmount = null;
        if (command.FreeAboveAmount.HasValue)
        {
            var freeAboveAmountResult = Money.Create(command.FreeAboveAmount.Value);
            if (freeAboveAmountResult.IsFailure)
                return Result.Failure<Guid>(freeAboveAmountResult.Error);
            freeAboveAmount = freeAboveAmountResult.Value;
        }

        Money? feePerKm = null;
        if (command.FeePerKm.HasValue)
        {
            var feePerKmResult = Money.Create(command.FeePerKm.Value);
            if (feePerKmResult.IsFailure)
                return Result.Failure<Guid>(feePerKmResult.Error);
            feePerKm = feePerKmResult.Value;
        }

        Money? minimumFee = null;
        if (command.MinimumFee.HasValue)
        {
            var minimumFeeResult = Money.Create(command.MinimumFee.Value);
            if (minimumFeeResult.IsFailure)
                return Result.Failure<Guid>(minimumFeeResult.Error);
            minimumFee = minimumFeeResult.Value;
        }

        Money? maximumFee = null;
        if (command.MaximumFee.HasValue)
        {
            var maximumFeeResult = Money.Create(command.MaximumFee.Value);
            if (maximumFeeResult.IsFailure)
                return Result.Failure<Guid>(maximumFeeResult.Error);
            maximumFee = maximumFeeResult.Value;
        }

        var settingsResult = DeliverySettings.Create(
            tenantId.Value,
            command.FeeType,
            fixedFee,
            freeAboveAmount,
            feePerKm,
            minimumFee,
            maximumFee);

        if (settingsResult.IsFailure)
            return Result.Failure<Guid>(settingsResult.Error);

        var settings = settingsResult.Value;
        await _deliverySettingsRepository.AddAsync(settings, cancellationToken);

        return Result.Success(settings.Id);
    }
}
