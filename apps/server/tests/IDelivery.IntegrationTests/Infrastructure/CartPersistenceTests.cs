using FluentAssertions;
using IDelivery.Domain.Carts.Entities;
using IDelivery.Domain.Common.ValueObjects;
using IDelivery.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IDelivery.IntegrationTests.Infrastructure;

public class CartPersistenceTests
{
    private readonly DbContextOptions<ApplicationDbContext> _dbContextOptions;
    private readonly Guid _tenantId = Guid.NewGuid();

    public CartPersistenceTests()
    {
        _dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task Add_ValidCartWithItems_ShouldPersist()
    {
        using var context = new ApplicationDbContext(_dbContextOptions);
        var cartResult = Cart.Create(_tenantId, Guid.NewGuid());
        Assert.True(cartResult.IsSuccess, cartResult.Error.Message);
        var cart = cartResult.Value;

        var addItemResult = cart.AddItem(
            Guid.NewGuid(),
            "Coca-Cola",
            Money.Create(5.50m, "BRL").Value,
            2);
        Assert.True(addItemResult.IsSuccess, addItemResult.Error.Message);

        context.Carts.Add(cart);
        foreach (var item in cart.Items)
            context.CartItems.Add(item);
        await context.SaveChangesAsync();

        var savedCart = await context.Carts.FindAsync(cart.Id);
        savedCart.Should().NotBeNull();
        savedCart.TenantId.Should().Be(_tenantId);
        savedCart.UserId.Should().NotBeNull();
        savedCart.CreatedAt.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));

        var savedItems = await context.CartItems.Where(ci => ci.CartId == cart.Id).ToListAsync();
        savedItems.Should().HaveCount(1);
        savedItems[0].ProductName.Should().Be("Coca-Cola");
        savedItems[0].Quantity.Should().Be(2);
    }

    [Fact]
    public async Task Get_ExistingCart_ShouldReturn()
    {
        using var context = new ApplicationDbContext(_dbContextOptions);
        var cartResult = Cart.Create(_tenantId, null, "session-123");
        Assert.True(cartResult.IsSuccess);
        var cart = cartResult.Value;
        context.Carts.Add(cart);
        await context.SaveChangesAsync();

        var found = await context.Carts.FirstAsync(c => c.Id == cart.Id);

        found.Should().NotBeNull();
        found.SessionId.Should().Be("session-123");
        found.UserId.Should().BeNull();
    }

    [Fact]
    public async Task Update_CartItems_ShouldUpdate()
    {
        using var context = new ApplicationDbContext(_dbContextOptions);
        var cartResult = Cart.Create(_tenantId, Guid.NewGuid());
        Assert.True(cartResult.IsSuccess);
        var cart = cartResult.Value;

        var productId = Guid.NewGuid();
        cart.AddItem(productId, "Pepsi", Money.Create(6.00m, "BRL").Value, 1);
        context.Carts.Add(cart);
        foreach (var item in cart.Items)
            context.CartItems.Add(item);
        await context.SaveChangesAsync();

        cart.UpdateItemQuantity(productId, 5);
        foreach (var item in cart.Items)
            context.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        await context.SaveChangesAsync();

        var updatedItem = await context.CartItems.FirstAsync(ci => ci.CartId == cart.Id && ci.ProductId == productId);
        updatedItem.Should().NotBeNull();
        updatedItem.Quantity.Should().Be(5);
    }

    [Fact]
    public async Task CartItem_Subtotal_ShouldCalculateCorrectly()
    {
        using var context = new ApplicationDbContext(_dbContextOptions);
        var cartResult = Cart.Create(_tenantId, Guid.NewGuid());
        Assert.True(cartResult.IsSuccess);
        var cart = cartResult.Value;

        var addItemResult = cart.AddItem(
            Guid.NewGuid(),
            "Coca-Cola",
            Money.Create(5.50m, "BRL").Value,
            3);
        Assert.True(addItemResult.IsSuccess);
        context.Carts.Add(cart);
        foreach (var item in cart.Items)
            context.CartItems.Add(item);
        await context.SaveChangesAsync();

        var savedItem = await context.CartItems.FirstAsync(ci => ci.CartId == cart.Id);
        savedItem.Should().NotBeNull();
        savedItem.Subtotal.Amount.Should().Be(16.50m);
        savedItem.Subtotal.Currency.Should().Be("BRL");
    }
}
