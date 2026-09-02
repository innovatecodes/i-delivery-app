using IDelivery.SharedKernel.Common.Result;
using IDelivery.Domain.Common.ValueObjects;
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
    public Money FixedFee { get; private set; } = null!;
    public Money? FreeAboveAmount { get; private set; }
    public Money? FeePerKm { get; private set; }
    public Money? MinimumFee { get; private set; }
    public Money? MaximumFee { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private DeliverySettings() { }

    private DeliverySettings(
        Guid id,
        Guid tenantId,
        DeliveryFeeType feeType,
        Money fixedFee,
        Money? freeAboveAmount,
        Money? feePerKm,
        Money? minimumFee,
        Money? maximumFee) : base(id)
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
        Money fixedFee,
        Money? freeAboveAmount = null,
        Money? feePerKm = null,
        Money? minimumFee = null,
        Money? maximumFee = null)
    {
        if (tenantId == Guid.Empty)
            return Result.Failure<DeliverySettings>(new Error("DeliverySettings.TenantRequired", "Tenant é obrigatório"));

        if (fixedFee.Amount < 0)
            return Result.Failure<DeliverySettings>(new Error("DeliverySettings.FixedFeeNegative", "Taxa fixa não pode ser negativa"));

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
        Money fixedFee,
        Money? freeAboveAmount,
        Money? feePerKm,
        Money? minimumFee,
        Money? maximumFee)
    {
        if (fixedFee.Amount < 0)
            return Result.Failure(new Error("DeliverySettings.FixedFeeNegative", "Taxa fixa não pode ser negativa"));

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
    public Money CalculateFee(Money orderAmount, decimal? distanceKm = null)
    {
        if (!IsActive)
            return Money.Zero(orderAmount.Currency);

        Money fee = FeeType switch
        {
            DeliveryFeeType.Free => Money.Zero(orderAmount.Currency),
            DeliveryFeeType.FreeAboveAmount => orderAmount.Amount >= (FreeAboveAmount?.Amount ?? 0) ? Money.Zero(orderAmount.Currency) : FixedFee,
            DeliveryFeeType.Fixed => FixedFee,
            DeliveryFeeType.PerDistance => CalculateDistanceFee(distanceKm, orderAmount.Currency),
            _ => Money.Zero(orderAmount.Currency)
        };

        // Aplica limites se configurados
        if (MinimumFee is not null && fee.Amount < MinimumFee.Amount)
            fee = MinimumFee;

        if (MaximumFee is not null && fee.Amount > MaximumFee.Amount)
            fee = MaximumFee;

        return fee;
    }

    private Money CalculateDistanceFee(decimal? distanceKm, string currency)
    {
        if (!distanceKm.HasValue || distanceKm <= 0)
            return FixedFee; // Fallback para taxa fixa se não há distância

        var fee = distanceKm.Value * (FeePerKm?.Amount ?? 0);

        // Garante taxa mínima
        if (fee < FixedFee.Amount)
            fee = FixedFee.Amount;

        return Money.Create(fee, currency).Value;
    }

    private static Result ValidateFeeConfiguration(
        DeliveryFeeType feeType,
        Money fixedFee,
        Money? freeAboveAmount,
        Money? feePerKm,
        Money? minimumFee,
        Money? maximumFee)
    {
        if (fixedFee.Amount < 0)
            return Result.Failure(new Error("DeliverySettings.FixedFeeNegative", "Taxa fixa não pode ser negativa"));

        return feeType switch
        {
            DeliveryFeeType.Free => Result.Success(),
            DeliveryFeeType.FreeAboveAmount => freeAboveAmount is not null && freeAboveAmount.Amount > 0
                ? Result.Success()
                : Result.Failure(new Error("DeliverySettings.FreeAboveAmountRequired", "Valor para entrega grátis é obrigatório")),
            DeliveryFeeType.Fixed => Result.Success(),
            DeliveryFeeType.PerDistance => feePerKm is not null && feePerKm.Amount > 0
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