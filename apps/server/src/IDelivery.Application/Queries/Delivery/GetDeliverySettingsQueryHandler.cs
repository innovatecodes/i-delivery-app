using IDelivery.Application.Abstractions.Authentication;
using IDelivery.Application.Abstractions.CQRS;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.Domain.Delivery.Enums;
using IDelivery.SharedKernel.Common.Result;

namespace IDelivery.Application.Queries.Delivery;

public sealed class GetDeliverySettingsQueryHandler : IQueryHandler<GetDeliverySettingsQuery, DeliverySettingsResponse>
{
    private readonly IDeliverySettingsRepository _deliverySettingsRepository;
    private readonly ITenantContext _tenantContext;

    public GetDeliverySettingsQueryHandler(
        IDeliverySettingsRepository deliverySettingsRepository,
        ITenantContext tenantContext)
    {
        _deliverySettingsRepository = deliverySettingsRepository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<DeliverySettingsResponse>> Handle(GetDeliverySettingsQuery query, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantContext.TenantId;
        if (!tenantId.HasValue)
            return Result.Failure<DeliverySettingsResponse>(new Error("DeliverySettings.TenantRequired", "Tenant é obrigatório"));

        var settings = await _deliverySettingsRepository.GetByTenantIdAsync(tenantId.Value, cancellationToken);
        if (settings is null)
            return Result.Failure<DeliverySettingsResponse>(new Error("DeliverySettings.NotFound", "Configurações de entrega não encontradas"));

        var response = new DeliverySettingsResponse(
            settings.Id,
            settings.TenantId,
            settings.FeeType,
            settings.FixedFee.Amount,
            settings.FreeAboveAmount?.Amount,
            settings.FeePerKm?.Amount,
            settings.MinimumFee?.Amount,
            settings.MaximumFee?.Amount,
            settings.IsActive,
            settings.CreatedAt,
            settings.UpdatedAt);

        return Result.Success(response);
    }
}