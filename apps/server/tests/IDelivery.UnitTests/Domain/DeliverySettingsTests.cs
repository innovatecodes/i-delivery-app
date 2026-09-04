using System.Reflection;
using FluentAssertions;
using IDelivery.Domain.Common.ValueObjects;
using IDelivery.Domain.Delivery.Entities;
using IDelivery.Domain.Delivery.Enums;
using IDelivery.Domain.Delivery.Events;
using Xunit;

namespace IDelivery.UnitTests.Domain;

public class DeliverySettingsTests
{
    private readonly Guid _tenantId = Guid.NewGuid();

    // ── Create ──────────────────────────────────────────────────────────

    [Fact]
    public void Create_WithFixedFeeType_ShouldCreateSettings()
    {
        var result = DeliverySettings.Create(
            _tenantId,
            DeliveryFeeType.Fixed,
            Money.Create(5.00m).Value);

        result.IsSuccess.Should().BeTrue();
        var settings = result.Value;
        settings.Id.Should().NotBeEmpty();
        settings.TenantId.Should().Be(_tenantId);
        settings.FeeType.Should().Be(DeliveryFeeType.Fixed);
        settings.FixedFee.Amount.Should().Be(5.00m);
        settings.IsActive.Should().BeTrue();
        settings.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_WithFreeFeeType_ShouldCreateSettings()
    {
        var result = DeliverySettings.Create(
            _tenantId,
            DeliveryFeeType.Free,
            Money.Zero());

        result.IsSuccess.Should().BeTrue();
        result.Value.FeeType.Should().Be(DeliveryFeeType.Free);
    }

    [Fact]
    public void Create_WithFreeAboveAmountType_ShouldCreateSettings()
    {
        var result = DeliverySettings.Create(
            _tenantId,
            DeliveryFeeType.FreeAboveAmount,
            Money.Create(8.00m).Value,
            freeAboveAmount: Money.Create(50.00m).Value);

        result.IsSuccess.Should().BeTrue();
        result.Value.FeeType.Should().Be(DeliveryFeeType.FreeAboveAmount);
        result.Value.FixedFee.Amount.Should().Be(8.00m);
        result.Value.FreeAboveAmount!.Amount.Should().Be(50.00m);
    }

    [Fact]
    public void Create_WithPerDistanceType_ShouldCreateSettings()
    {
        var result = DeliverySettings.Create(
            _tenantId,
            DeliveryFeeType.PerDistance,
            Money.Create(3.00m).Value,
            feePerKm: Money.Create(2.50m).Value);

        result.IsSuccess.Should().BeTrue();
        result.Value.FeeType.Should().Be(DeliveryFeeType.PerDistance);
        result.Value.FeePerKm!.Amount.Should().Be(2.50m);
    }

    [Fact]
    public void Create_WithEmptyTenantId_ShouldFail()
    {
        var result = DeliverySettings.Create(
            Guid.Empty,
            DeliveryFeeType.Fixed,
            Money.Create(5.00m).Value);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("DeliverySettings.TenantRequired");
    }

    [Fact]
    public void Create_WithNegativeFixedFee_ShouldFail()
    {
        var result = DeliverySettings.Create(
            _tenantId,
            DeliveryFeeType.Fixed,
            CreateNegativeMoney(-1.00m));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("DeliverySettings.FixedFeeNegative");
    }

    [Fact]
    public void Create_FreeAboveAmountType_WithoutFreeAboveAmount_ShouldFail()
    {
        var result = DeliverySettings.Create(
            _tenantId,
            DeliveryFeeType.FreeAboveAmount,
            Money.Create(5.00m).Value);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("DeliverySettings.FreeAboveAmountRequired");
    }

    [Fact]
    public void Create_FreeAboveAmountType_WithZeroFreeAboveAmount_ShouldFail()
    {
        var result = DeliverySettings.Create(
            _tenantId,
            DeliveryFeeType.FreeAboveAmount,
            Money.Create(5.00m).Value,
            freeAboveAmount: Money.Zero());

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("DeliverySettings.FreeAboveAmountRequired");
    }

    [Fact]
    public void Create_PerDistanceType_WithoutFeePerKm_ShouldFail()
    {
        var result = DeliverySettings.Create(
            _tenantId,
            DeliveryFeeType.PerDistance,
            Money.Create(3.00m).Value);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("DeliverySettings.FeePerKmRequired");
    }

    [Fact]
    public void Create_PerDistanceType_WithZeroFeePerKm_ShouldFail()
    {
        var result = DeliverySettings.Create(
            _tenantId,
            DeliveryFeeType.PerDistance,
            Money.Create(3.00m).Value,
            feePerKm: Money.Zero());

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("DeliverySettings.FeePerKmRequired");
    }

    [Fact]
    public void Create_ShouldRaiseDeliverySettingsCreatedDomainEvent()
    {
        var result = DeliverySettings.Create(
            _tenantId,
            DeliveryFeeType.Fixed,
            Money.Create(5.00m).Value);

        result.IsSuccess.Should().BeTrue();
        result.Value.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<DeliverySettingsCreatedDomainEvent>();
        var domainEvent = (DeliverySettingsCreatedDomainEvent)result.Value.DomainEvents.First();
        domainEvent.DeliverySettingsId.Should().Be(result.Value.Id);
        domainEvent.TenantId.Should().Be(_tenantId);
        domainEvent.FeeType.Should().Be(DeliveryFeeType.Fixed);
    }

    // ── Update ──────────────────────────────────────────────────────────

    [Fact]
    public void Update_WithValidData_ShouldUpdateSettings()
    {
        var settings = CreateDefaultSettings();

        var result = settings.Update(
            DeliveryFeeType.Free,
            Money.Zero(),
            null,
            null,
            null,
            null);

        result.IsSuccess.Should().BeTrue();
        settings.FeeType.Should().Be(DeliveryFeeType.Free);
        settings.UpdatedAt.Should().NotBeNull();
        settings.UpdatedAt!.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Update_WithNegativeFixedFee_ShouldFail()
    {
        var settings = CreateDefaultSettings();

        var result = settings.Update(
            DeliveryFeeType.Fixed,
            CreateNegativeMoney(-2.00m),
            null,
            null,
            null,
            null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("DeliverySettings.FixedFeeNegative");
    }

    [Fact]
    public void Update_FreeAboveAmountType_WithoutFreeAboveAmount_ShouldFail()
    {
        var settings = CreateDefaultSettings();

        var result = settings.Update(
            DeliveryFeeType.FreeAboveAmount,
            Money.Create(5.00m).Value,
            null,
            null,
            null,
            null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("DeliverySettings.FreeAboveAmountRequired");
    }

    [Fact]
    public void Update_PerDistanceType_WithoutFeePerKm_ShouldFail()
    {
        var settings = CreateDefaultSettings();

        var result = settings.Update(
            DeliveryFeeType.PerDistance,
            Money.Create(3.00m).Value,
            null,
            null,
            null,
            null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("DeliverySettings.FeePerKmRequired");
    }

    [Fact]
    public void Update_ShouldRaiseDeliverySettingsUpdatedDomainEvent()
    {
        var settings = CreateDefaultSettings();
        settings.ClearDomainEvents();

        settings.Update(
            DeliveryFeeType.Free,
            Money.Zero(),
            null,
            null,
            null,
            null);

        settings.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<DeliverySettingsUpdatedDomainEvent>();
        var domainEvent = (DeliverySettingsUpdatedDomainEvent)settings.DomainEvents.First();
        domainEvent.DeliverySettingsId.Should().Be(settings.Id);
        domainEvent.TenantId.Should().Be(_tenantId);
        domainEvent.FeeType.Should().Be(DeliveryFeeType.Free);
    }

    // ── CalculateFee ────────────────────────────────────────────────────

    [Fact]
    public void CalculateFee_FreeType_ShouldReturnZero()
    {
        var settings = CreateSettingsWithFeeType(DeliveryFeeType.Free);

        var fee = settings.CalculateFee(Money.Create(100.00m).Value);

        fee.Amount.Should().Be(0);
    }

    [Fact]
    public void CalculateFee_FixedType_ShouldReturnFixedFee()
    {
        var settings = CreateSettingsWithFeeType(DeliveryFeeType.Fixed, fixedFee: 7.50m);

        var fee = settings.CalculateFee(Money.Create(100.00m).Value);

        fee.Amount.Should().Be(7.50m);
    }

    [Fact]
    public void CalculateFee_FreeAboveAmountType_OrderAboveThreshold_ShouldReturnZero()
    {
        var settings = DeliverySettings.Create(
            _tenantId,
            DeliveryFeeType.FreeAboveAmount,
            Money.Create(5.00m).Value,
            freeAboveAmount: Money.Create(50.00m).Value).Value;

        var fee = settings.CalculateFee(Money.Create(60.00m).Value);

        fee.Amount.Should().Be(0);
    }

    [Fact]
    public void CalculateFee_FreeAboveAmountType_OrderBelowThreshold_ShouldReturnFixedFee()
    {
        var settings = DeliverySettings.Create(
            _tenantId,
            DeliveryFeeType.FreeAboveAmount,
            Money.Create(5.00m).Value,
            freeAboveAmount: Money.Create(50.00m).Value).Value;

        var fee = settings.CalculateFee(Money.Create(30.00m).Value);

        fee.Amount.Should().Be(5.00m);
    }

    [Fact]
    public void CalculateFee_FreeAboveAmountType_OrderExactlyAtThreshold_ShouldReturnZero()
    {
        var settings = DeliverySettings.Create(
            _tenantId,
            DeliveryFeeType.FreeAboveAmount,
            Money.Create(5.00m).Value,
            freeAboveAmount: Money.Create(50.00m).Value).Value;

        var fee = settings.CalculateFee(Money.Create(50.00m).Value);

        fee.Amount.Should().Be(0);
    }

    [Fact]
    public void CalculateFee_PerDistanceType_ShouldCalculateBasedOnDistance()
    {
        var settings = DeliverySettings.Create(
            _tenantId,
            DeliveryFeeType.PerDistance,
            Money.Create(3.00m).Value,
            feePerKm: Money.Create(2.00m).Value).Value;

        var fee = settings.CalculateFee(Money.Create(100.00m).Value, distanceKm: 5m);

        // 5 km * 2.00 = 10.00, which is >= fixedFee 3.00
        fee.Amount.Should().Be(10.00m);
    }

    [Fact]
    public void CalculateFee_PerDistanceType_DistanceBelowFixedFee_ShouldReturnFixedFee()
    {
        var settings = DeliverySettings.Create(
            _tenantId,
            DeliveryFeeType.PerDistance,
            Money.Create(5.00m).Value,
            feePerKm: Money.Create(1.00m).Value).Value;

        var fee = settings.CalculateFee(Money.Create(50.00m).Value, distanceKm: 2m);

        // 2 km * 1.00 = 2.00, which is < fixedFee 5.00, so fallback to fixedFee
        fee.Amount.Should().Be(5.00m);
    }

    [Fact]
    public void CalculateFee_PerDistanceType_WithoutDistance_ShouldFallbackToFixedFee()
    {
        var settings = DeliverySettings.Create(
            _tenantId,
            DeliveryFeeType.PerDistance,
            Money.Create(5.00m).Value,
            feePerKm: Money.Create(2.00m).Value).Value;

        var fee = settings.CalculateFee(Money.Create(100.00m).Value);

        fee.Amount.Should().Be(5.00m);
    }

    [Fact]
    public void CalculateFee_WhenInactive_ShouldReturnZero()
    {
        var settings = CreateDefaultSettings();
        settings.Deactivate();

        var fee = settings.CalculateFee(Money.Create(100.00m).Value);

        fee.Amount.Should().Be(0);
    }

    [Fact]
    public void CalculateFee_FixedType_WithMinimumFee_FeeBelowMinimum_ShouldBumpToMinimum()
    {
        var settings = DeliverySettings.Create(
            _tenantId,
            DeliveryFeeType.Fixed,
            Money.Create(2.00m).Value,
            minimumFee: Money.Create(5.00m).Value).Value;

        var fee = settings.CalculateFee(Money.Create(100.00m).Value);

        fee.Amount.Should().Be(5.00m);
    }

    [Fact]
    public void CalculateFee_FixedType_WithMinimumFee_FeeAboveMinimum_ShouldReturnFixedFee()
    {
        var settings = DeliverySettings.Create(
            _tenantId,
            DeliveryFeeType.Fixed,
            Money.Create(8.00m).Value,
            minimumFee: Money.Create(5.00m).Value).Value;

        var fee = settings.CalculateFee(Money.Create(100.00m).Value);

        fee.Amount.Should().Be(8.00m);
    }

    [Fact]
    public void CalculateFee_FixedType_WithMaximumFee_FeeAboveMaximum_ShouldCapAtMaximum()
    {
        var settings = DeliverySettings.Create(
            _tenantId,
            DeliveryFeeType.Fixed,
            Money.Create(20.00m).Value,
            maximumFee: Money.Create(10.00m).Value).Value;

        var fee = settings.CalculateFee(Money.Create(100.00m).Value);

        fee.Amount.Should().Be(10.00m);
    }

    [Fact]
    public void CalculateFee_FixedType_WithMaximumFee_FeeBelowMaximum_ShouldReturnFixedFee()
    {
        var settings = DeliverySettings.Create(
            _tenantId,
            DeliveryFeeType.Fixed,
            Money.Create(6.00m).Value,
            maximumFee: Money.Create(10.00m).Value).Value;

        var fee = settings.CalculateFee(Money.Create(100.00m).Value);

        fee.Amount.Should().Be(6.00m);
    }

    [Fact]
    public void CalculateFee_WithBothMinimumAndMaximum_ShouldClampFee()
    {
        var settings = DeliverySettings.Create(
            _tenantId,
            DeliveryFeeType.Fixed,
            Money.Create(2.00m).Value,
            minimumFee: Money.Create(5.00m).Value,
            maximumFee: Money.Create(10.00m).Value).Value;

        var fee = settings.CalculateFee(Money.Create(100.00m).Value);

        fee.Amount.Should().Be(5.00m);
    }

    // ── Activate / Deactivate ───────────────────────────────────────────

    [Fact]
    public void Activate_WhenInactive_ShouldActivate()
    {
        var settings = CreateDefaultSettings();
        settings.Deactivate();

        var result = settings.Activate();

        result.IsSuccess.Should().BeTrue();
        settings.IsActive.Should().BeTrue();
        settings.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Activate_WhenAlreadyActive_ShouldFail()
    {
        var settings = CreateDefaultSettings();

        var result = settings.Activate();

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("DeliverySettings.AlreadyActive");
    }

    [Fact]
    public void Deactivate_WhenActive_ShouldDeactivate()
    {
        var settings = CreateDefaultSettings();

        var result = settings.Deactivate();

        result.IsSuccess.Should().BeTrue();
        settings.IsActive.Should().BeFalse();
        settings.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_ShouldFail()
    {
        var settings = CreateDefaultSettings();
        settings.Deactivate();

        var result = settings.Deactivate();

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("DeliverySettings.AlreadyInactive");
    }

    // ── Helpers ─────────────────────────────────────────────────────────

    private DeliverySettings CreateDefaultSettings()
    {
        return DeliverySettings.Create(
            _tenantId,
            DeliveryFeeType.Fixed,
            Money.Create(5.00m).Value).Value;
    }

    private DeliverySettings CreateSettingsWithFeeType(
        DeliveryFeeType feeType,
        decimal fixedFee = 0,
        decimal? freeAboveAmount = null,
        decimal? feePerKm = null,
        decimal? minimumFee = null,
        decimal? maximumFee = null)
    {
        return DeliverySettings.Create(
            _tenantId,
            feeType,
            Money.Create(fixedFee).Value,
            freeAboveAmount.HasValue ? Money.Create(freeAboveAmount.Value).Value : null,
            feePerKm.HasValue ? Money.Create(feePerKm.Value).Value : null,
            minimumFee.HasValue ? Money.Create(minimumFee.Value).Value : null,
            maximumFee.HasValue ? Money.Create(maximumFee.Value).Value : null).Value;
    }

    private static Money CreateNegativeMoney(decimal amount, string currency = "BRL")
    {
        var ctor = typeof(Money).GetConstructor(
            BindingFlags.NonPublic | BindingFlags.Instance,
            null,
            new[] { typeof(decimal), typeof(string) },
            null);
        return (Money)ctor!.Invoke(new object[] { amount, currency });
    }
}
