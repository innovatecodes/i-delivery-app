using FluentAssertions;
using IDelivery.Domain.Common.ValueObjects;
using IDelivery.Domain.Orders.Entities;
using IDelivery.Domain.Orders.Enums;
using IDelivery.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IDelivery.IntegrationTests.Infrastructure;

public class OrderPersistenceTests
{
    private readonly DbContextOptions<ApplicationDbContext> _dbContextOptions;
    private readonly Guid _tenantId = Guid.NewGuid();

    public OrderPersistenceTests()
    {
        _dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    private Address CreateTestAddress()
    {
        return new Address(
            "Rua das Flores",
            "123",
            null,
            "Centro",
            "São Paulo",
            "SP",
            ZipCode.Create("01001-000").Value);
    }

    [Fact]
    public async Task Add_ValidOrderWithItems_ShouldPersist()
    {
        using var context = new ApplicationDbContext(_dbContextOptions);
        var productId = Guid.NewGuid();
        var itemResult = OrderItem.Create(Guid.NewGuid(), productId, "Coca-Cola", Money.Create(5.50m, "BRL").Value, 2);
        Assert.True(itemResult.IsSuccess, itemResult.Error.Message);
        var orderItem = itemResult.Value;

        var orderResult = Order.Create(
            _tenantId,
            Guid.NewGuid(),
            [orderItem],
            Money.Create(5.00m, "BRL").Value,
            CreateTestAddress());
        Assert.True(orderResult.IsSuccess, orderResult.Error.Message);
        var order = orderResult.Value;

        context.OrderItems.Add(orderItem);
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var savedOrder = await context.Orders.FindAsync(order.Id);
        savedOrder.Should().NotBeNull();
        savedOrder.TenantId.Should().Be(_tenantId);
        savedOrder.State.Should().Be(OrderState.Pending);
        savedOrder.Subtotal.Amount.Should().Be(11.00m);
        savedOrder.DeliveryFee.Amount.Should().Be(5.00m);
        savedOrder.TotalAmount.Amount.Should().Be(16.00m);
        savedOrder.DeliveryAddress.Should().NotBeNull();
        savedOrder.DeliveryAddress!.Street.Should().Be("Rua das Flores");
    }

    [Fact]
    public async Task Get_ExistingOrder_ShouldReturn()
    {
        using var context = new ApplicationDbContext(_dbContextOptions);
        var itemResult = OrderItem.Create(Guid.NewGuid(), Guid.NewGuid(), "Pepsi", Money.Create(6.00m, "BRL").Value, 1);
        Assert.True(itemResult.IsSuccess);
        var orderItem = itemResult.Value;

        var orderResult = Order.Create(
            _tenantId,
            Guid.NewGuid(),
            [orderItem],
            Money.Create(3.00m, "BRL").Value,
            CreateTestAddress());
        Assert.True(orderResult.IsSuccess);
        var order = orderResult.Value;
        context.OrderItems.Add(orderItem);
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var found = await context.Orders.FirstAsync(o => o.Id == order.Id);

        found.Should().NotBeNull();
        found.State.Should().Be(OrderState.Pending);
        found.TotalAmount.Amount.Should().Be(9.00m);
    }

    [Fact]
    public async Task Update_OrderState_ShouldUpdate()
    {
        using var context = new ApplicationDbContext(_dbContextOptions);
        var itemResult = OrderItem.Create(Guid.NewGuid(), Guid.NewGuid(), "Guaraná", Money.Create(4.00m, "BRL").Value, 3);
        Assert.True(itemResult.IsSuccess);
        var orderItem = itemResult.Value;

        var orderResult = Order.Create(
            _tenantId,
            Guid.NewGuid(),
            [orderItem],
            Money.Create(4.00m, "BRL").Value,
            CreateTestAddress());
        Assert.True(orderResult.IsSuccess);
        var order = orderResult.Value;
        context.OrderItems.Add(orderItem);
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var confirmResult = order.Confirm();
        Assert.True(confirmResult.IsSuccess);
        context.Entry(order).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        await context.SaveChangesAsync();

        var updated = await context.Orders.FindAsync(order.Id);
        updated?.State.Should().Be(OrderState.Confirmed);
        updated?.ConfirmedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task OrderItem_Subtotal_ShouldCalculateCorrectly()
    {
        using var context = new ApplicationDbContext(_dbContextOptions);
        var orderItemResult = OrderItem.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Coca-Cola",
            Money.Create(5.50m, "BRL").Value,
            3);
        Assert.True(orderItemResult.IsSuccess);
        var orderItem = orderItemResult.Value;

        var orderResult = Order.Create(
            _tenantId,
            Guid.NewGuid(),
            [orderItem],
            Money.Create(0m, "BRL").Value,
            CreateTestAddress());
        Assert.True(orderResult.IsSuccess);
        var order = orderResult.Value;
        context.OrderItems.Add(orderItem);
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var savedItem = await context.OrderItems.FindAsync(orderItem.Id);
        savedItem.Should().NotBeNull();
        savedItem.Subtotal.Amount.Should().Be(16.50m);
        savedItem.Subtotal.Currency.Should().Be("BRL");
    }
}
