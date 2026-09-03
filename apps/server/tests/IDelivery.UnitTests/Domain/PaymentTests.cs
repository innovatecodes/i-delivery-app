using FluentAssertions;
using IDelivery.Domain.Common.ValueObjects;
using IDelivery.Domain.Payments.Entities;
using IDelivery.Domain.Payments.Enums;
using IDelivery.Domain.Payments.Events;
using Xunit;

namespace IDelivery.UnitTests.Domain;

public class PaymentTests
{
    private readonly Guid _orderId = Guid.NewGuid();
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _customerId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        var amount = Money.Create(25.50m, "BRL").Value;

        var result = Payment.Create(_orderId, _tenantId, _customerId, amount, PaymentMethod.Cash);

        result.IsSuccess.Should().BeTrue();
        result.Value.OrderId.Should().Be(_orderId);
        result.Value.TenantId.Should().Be(_tenantId);
        result.Value.CustomerId.Should().Be(_customerId);
        result.Value.Amount.Amount.Should().Be(25.50m);
        result.Value.Method.Should().Be(PaymentMethod.Cash);
        result.Value.Status.Should().Be(PaymentStatus.Pending);
        result.Value.PaidAt.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldDispatchPaymentCreatedEvent()
    {
        var amount = Money.Create(25.50m, "BRL").Value;

        var result = Payment.Create(_orderId, _tenantId, _customerId, amount, PaymentMethod.Cash);

        result.Value.DomainEvents.Should().Contain(e => e is PaymentCreatedDomainEvent);
    }

    [Fact]
    public void Create_WithEmptyOrderId_ShouldFail()
    {
        var amount = Money.Create(25.50m, "BRL").Value;

        var result = Payment.Create(Guid.Empty, _tenantId, _customerId, amount, PaymentMethod.Cash);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Payment.OrderRequired");
    }

    [Fact]
    public void Create_WithEmptyTenantId_ShouldFail()
    {
        var amount = Money.Create(25.50m, "BRL").Value;

        var result = Payment.Create(_orderId, Guid.Empty, _customerId, amount, PaymentMethod.Cash);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Payment.TenantRequired");
    }

    [Fact]
    public void Create_WithEmptyCustomerId_ShouldFail()
    {
        var amount = Money.Create(25.50m, "BRL").Value;

        var result = Payment.Create(_orderId, _tenantId, Guid.Empty, amount, PaymentMethod.Cash);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Payment.CustomerRequired");
    }

    [Fact]
    public void Create_WithZeroAmount_ShouldFail()
    {
        var amount = Money.Create(0, "BRL").Value;

        var result = Payment.Create(_orderId, _tenantId, _customerId, amount, PaymentMethod.Cash);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Payment.InvalidAmount");
    }

    [Fact]
    public void MarkAsPaid_WhenPending_ShouldSucceed()
    {
        var payment = CreatePayment();

        var result = payment.MarkAsPaid();

        result.IsSuccess.Should().BeTrue();
        payment.Status.Should().Be(PaymentStatus.Paid);
        payment.PaidAt.Should().NotBeNull();
    }

    [Fact]
    public void MarkAsPaid_WhenPending_ShouldDispatchPaymentMarkedAsPaidEvent()
    {
        var payment = CreatePayment();

        payment.MarkAsPaid();

        payment.DomainEvents.Should().Contain(e => e is PaymentMarkedAsPaidDomainEvent);
    }

    [Fact]
    public void MarkAsPaid_WhenAlreadyPaid_ShouldFail()
    {
        var payment = CreatePayment();
        payment.MarkAsPaid();

        var result = payment.MarkAsPaid();

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Payment.InvalidStatus");
    }

    [Fact]
    public void MarkAsPaid_WhenNotCollected_ShouldFail()
    {
        var payment = CreatePayment();
        payment.MarkAsNotCollected();

        var result = payment.MarkAsPaid();

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Payment.InvalidStatus");
    }

    [Fact]
    public void MarkAsNotCollected_WhenPending_ShouldSucceed()
    {
        var payment = CreatePayment();

        var result = payment.MarkAsNotCollected();

        result.IsSuccess.Should().BeTrue();
        payment.Status.Should().Be(PaymentStatus.NotCollected);
    }

    [Fact]
    public void MarkAsNotCollected_WhenAlreadyPaid_ShouldFail()
    {
        var payment = CreatePayment();
        payment.MarkAsPaid();

        var result = payment.MarkAsNotCollected();

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Payment.InvalidStatus");
    }

    private Payment CreatePayment()
    {
        var amount = Money.Create(25.50m, "BRL").Value;
        return Payment.Create(_orderId, _tenantId, _customerId, amount, PaymentMethod.Cash).Value;
    }
}
