using IDelivery.SharedKernel.Common.Result;
using IDelivery.Domain.Common.Entities;
using IDelivery.Domain.Delivery.Events;
using IDelivery.Domain.Delivery.Enums;

namespace IDelivery.Domain.Delivery.Entities;

/// <summary>
/// Aggregate Root das configurações de entrega de um tenant.
/// Define como a taxa de entrega é calculada.
/// </summary>
public sealed class DeliverySettings : AggregateRoot
{
    public Guid TenantId { get; private set; }
    public DeliveryFeeType FeeType { get; private set; }
    public decimal FixedFee { get; private set; }
    public decimal? FreeAboveAmount { get; private set; }
    public decimal? FeePerKm { get; private set; }
    public decimal? MinimumFee { get; private set; }
    public decimal? MaximumFee { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private DeliverySettings() { }

    private DeliverySettings(
        Guid id,
        Guid tenantId,
        DeliveryFeeType feeType,
        decimal fixedFee,
        decimal? freeAboveAmount,
        decimal? feePerKm,
        decimal? minimumFee,
        decimal? maximumFee) : base(id)
    {
        TenantId = tenantId;
        FeeType = feeType;
        FixedFee = fixedFee;
        FreeAboveAmount = freeAboveAmount;
        FeePerKm = feePerKm;
        MinimumFee = minimumFee;
        MaximumFee = maximumFee;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;

        AddDomainEvent(new DeliverySettingsCreatedDomainEvent(id, tenantId, feeType));
    }

    /// <summary>
    /// Factory method para criar novas configurações de entrega.
    /// </summary>
    public static Result<DeliverySettings> Create(
        Guid tenantId,
        DeliveryFeeType feeType,
        decimal fixedFee = 0,
        decimal? freeAboveAmount = null,
        decimal? feePerKm = null,
        decimal? minimumFee = null,
        decimal? maximumFee = null)
    {
        if (tenantId == Guid.Empty)
            return Result.Failure<DeliverySettings>(new Error("DeliverySettings.TenantRequired", "Tenant é obrigatório"));

        // Validação baseada no tipo de taxa
        var validationResult = ValidateFeeConfiguration(feeType, fixedFee, freeAboveAmount, feePerKm, minimumFee, maximumFee);
        if (validationResult.IsFailure)
            return Result.Failure<DeliverySettings>(validationResult.Error);

        var settings = new DeliverySettings(
            Guid.NewGuid(),
            tenantId,
            feeType,
            fixedFee,
            freeAboveAmount,
            feePerKm,
            minimumFee,
            maximumFee);

        return Result.Success(settings);
    }

    /// <summary>
    /// Atualiza as configurações de entrega.
    /// </summary>
    public Result Update(
        DeliveryFeeType feeType,
        decimal fixedFee,
        decimal? freeAboveAmount,
        decimal? feePerKm,
        decimal? minimumFee,
        decimal? maximumFee)
    {
        var validationResult = ValidateFeeConfiguration(feeType, fixedFee, freeAboveAmount, feePerKm, minimumFee, maximumFee);
        if (validationResult.IsFailure)
            return Result.Failure(validationResult.Error);

        FeeType = feeType;
        FixedFee = fixedFee;
        FreeAboveAmount = freeAboveAmount;
        FeePerKm = feePerKm;
        MinimumFee = minimumFee;
        MaximumFee = maximumFee;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new DeliverySettingsUpdatedDomainEvent(Id, TenantId, FeeType));

        return Result.Success();
    }

    /// <summary>
    /// Calcula a taxa de entrega para um pedido.
    /// </summary>
    public decimal CalculateFee(decimal orderAmount, decimal? distanceKm = null)
    {
        if (!IsActive)
            return 0;

        decimal fee = FeeType switch
        {
            DeliveryFeeType.Free => 0,
            DeliveryFeeType.FreeAboveAmount => orderAmount >= (FreeAboveAmount ?? 0) ? 0 : FixedFee,
            DeliveryFeeType.Fixed => FixedFee,
            DeliveryFeeType.PerDistance => CalculateDistanceFee(distanceKm),
            _ => 0
        };

        // Aplica limites se configurados
        if (MinimumFee.HasValue && fee < MinimumFee.Value)
            fee = MinimumFee.Value;

        if (MaximumFee.HasValue && fee > MaximumFee.Value)
            fee = MaximumFee.Value;

        return fee;
    }

    private decimal CalculateDistanceFee(decimal? distanceKm)
    {
        if (!distanceKm.HasValue || distanceKm <= 0)
            return FixedFee; // Fallback para taxa fixa se não há distância

        var fee = distanceKm.Value * (FeePerKm ?? 0);

        // Garante taxa mínima
        if (fee < FixedFee)
            fee = FixedFee;

        return fee;
    }

    private static Result ValidateFeeConfiguration(
        DeliveryFeeType feeType,
        decimal fixedFee,
        decimal? freeAboveAmount,
        decimal? feePerKm,
        decimal? minimumFee,
        decimal? maximumFee)
    {
        if (fixedFee < 0)
            return Result.Failure(new Error("DeliverySettings.FixedFeeNegative", "Taxa fixa não pode ser negativa"));

        return feeType switch
        {
            DeliveryFeeType.Free => Result.Success(),
            DeliveryFeeType.FreeAboveAmount => freeAboveAmount.HasValue && freeAboveAmount > 0
                ? Result.Success()
                : Result.Failure(new Error("DeliverySettings.FreeAboveAmountRequired", "Valor para entrega grátis é obrigatório")),
            DeliveryFeeType.Fixed => Result.Success(),
            DeliveryFeeType.PerDistance => feePerKm.HasValue && feePerKm > 0
                ? Result.Success()
                : Result.Failure(new Error("DeliverySettings.FeePerKmRequired", "Taxa por km é obrigatória")),
            _ => Result.Failure(new Error("DeliverySettings.InvalidFeeType", "Tipo de taxa inválido"))
        };
    }

    public Result Activate()
    {
        if (IsActive)
            return Result.Failure(new Error("DeliverySettings.AlreadyActive", "Configurações já estão ativas"));

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result Deactivate()
    {
        if (!IsActive)
            return Result.Failure(new Error("DeliverySettings.AlreadyInactive", "Configurações já estão inativas"));

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }
}