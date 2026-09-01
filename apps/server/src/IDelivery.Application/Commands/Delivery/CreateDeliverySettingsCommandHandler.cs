using IDelivery.Application.Abstractions.Authentication;
using IDelivery.Application.Abstractions.CQRS;
using IDelivery.Application.Abstractions.Persistence;
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

        var settingsResult = DeliverySettings.Create(
            tenantId.Value,
            command.FeeType,
            command.FixedFee,
            command.FreeAboveAmount,
            command.FeePerKm,
            command.MinimumFee,
            command.MaximumFee);

        if (settingsResult.IsFailure)
            return Result.Failure<Guid>(settingsResult.Error);

        var settings = settingsResult.Value;
        await _deliverySettingsRepository.AddAsync(settings, cancellationToken);

        return Result.Success(settings.Id);
    }
}