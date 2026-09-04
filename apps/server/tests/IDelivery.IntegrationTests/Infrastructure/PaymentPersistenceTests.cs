using FluentAssertions;
using IDelivery.Domain.Common.ValueObjects;
using IDelivery.Domain.Payments.Entities;
using IDelivery.Domain.Payments.Enums;
using IDelivery.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IDelivery.IntegrationTests.Infrastructure;

public class PaymentPersistenceTests
{
    private readonly DbContextOptions<ApplicationDbContext> _dbContextOptions;
    private readonly Guid _tenantId = Guid.NewGuid();

    public PaymentPersistenceTests()
    {
        _dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task Add_ValidPayment_ShouldPersist()
    {
        using var context = new ApplicationDbContext(_dbContextOptions);
        var paymentResult = Payment.Create(
            Guid.NewGuid(),
            _tenantId,
            Guid.NewGuid(),
            Money.Create(25.50m, "BRL").Value,
            PaymentMethod.Cash);
        Assert.True(paymentResult.IsSuccess, paymentResult.Error.Message);
        var payment = paymentResult.Value;

        context.Payments.Add(payment);
        await context.SaveChangesAsync();

        var savedPayment = await context.Payments.FindAsync(payment.Id);
        savedPayment.Should().NotBeNull();
        savedPayment.OrderId.Should().Be(payment.OrderId);
        savedPayment.TenantId.Should().Be(_tenantId);
        savedPayment.Amount.Amount.Should().Be(25.50m);
        savedPayment.Amount.Currency.Should().Be("BRL");
        savedPayment.Method.Should().Be(PaymentMethod.Cash);
        savedPayment.Status.Should().Be(PaymentStatus.Pending);
    }

    [Fact]
    public async Task Get_ExistingPayment_ShouldReturn()
    {
        using var context = new ApplicationDbContext(_dbContextOptions);
        var paymentResult = Payment.Create(
            Guid.NewGuid(),
            _tenantId,
            Guid.NewGuid(),
            Money.Create(10.00m, "BRL").Value,
            PaymentMethod.CardOnDelivery);
        Assert.True(paymentResult.IsSuccess);
        var payment = paymentResult.Value;
        context.Payments.Add(payment);
        await context.SaveChangesAsync();

        var found = await context.Payments.FirstAsync(p => p.OrderId == payment.OrderId && p.TenantId == _tenantId);

        found.Should().NotBeNull();
        found.Amount.Amount.Should().Be(10.00m);
        found.Method.Should().Be(PaymentMethod.CardOnDelivery);
    }

    [Fact]
    public async Task Update_PaymentStatus_ShouldUpdate()
    {
        using var context = new ApplicationDbContext(_dbContextOptions);
        var paymentResult = Payment.Create(
            Guid.NewGuid(),
            _tenantId,
            Guid.NewGuid(),
            Money.Create(50.00m, "BRL").Value,
            PaymentMethod.Cash);
        Assert.True(paymentResult.IsSuccess);
        var payment = paymentResult.Value;
        context.Payments.Add(payment);
        await context.SaveChangesAsync();

        var markPaidResult = payment.MarkAsPaid();
        Assert.True(markPaidResult.IsSuccess);
        context.Entry(payment).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        await context.SaveChangesAsync();

        var updated = await context.Payments.FindAsync(payment.Id);
        updated?.Status.Should().Be(PaymentStatus.Paid);
        updated?.PaidAt.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByOrderId_ExistingPayment_ShouldReturn()
    {
        using var context = new ApplicationDbContext(_dbContextOptions);
        var orderId = Guid.NewGuid();
        var payment1Result = Payment.Create(orderId, _tenantId, Guid.NewGuid(), Money.Create(30.00m, "BRL").Value, PaymentMethod.Cash);
        var payment2Result = Payment.Create(Guid.NewGuid(), _tenantId, Guid.NewGuid(), Money.Create(15.00m, "BRL").Value, PaymentMethod.CardOnDelivery);

        Assert.True(payment1Result.IsSuccess && payment2Result.IsSuccess);

        context.Payments.AddRange(payment1Result.Value, payment2Result.Value);
        await context.SaveChangesAsync();

        var found = await context.Payments.Where(p => p.OrderId == orderId).ToListAsync();

        found.Should().HaveCount(1);
        found[0].Amount.Amount.Should().Be(30.00m);
    }
}
