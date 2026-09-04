using System.Reflection;
using FluentAssertions;
using IDelivery.Domain.Carts.Entities;
using IDelivery.Domain.Common.ValueObjects;
using Xunit;

namespace IDelivery.UnitTests.Domain;

public class CartTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _productId = Guid.NewGuid();
    private readonly Money _price = Money.Create(25.50m, "BRL").Value;

    #region Cart.Create

    [Fact]
    public void Create_WithUserId_ShouldSucceed()
    {
        var result = Cart.Create(_tenantId, userId: _userId);

        result.IsSuccess.Should().BeTrue();
        var cart = result.Value;
        cart.Id.Should().NotBeEmpty();
        cart.TenantId.Should().Be(_tenantId);
        cart.UserId.Should().Be(_userId);
        cart.SessionId.Should().BeNull();
        cart.Items.Should().BeEmpty();
        cart.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_WithSessionId_ShouldSucceed()
    {
        var result = Cart.Create(_tenantId, sessionId: "session-abc-123");

        result.IsSuccess.Should().BeTrue();
        var cart = result.Value;
        cart.Id.Should().NotBeEmpty();
        cart.TenantId.Should().Be(_tenantId);
        cart.UserId.Should().BeNull();
        cart.SessionId.Should().Be("session-abc-123");
    }

    [Fact]
    public void Create_WithEmptyTenantId_ShouldFail()
    {
        var result = Cart.Create(Guid.Empty, userId: _userId);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Cart.TenantRequired");
    }

    [Fact]
    public void Create_WithBothUserIdAndSessionIdNull_ShouldFail()
    {
        var result = Cart.Create(_tenantId, userId: null, sessionId: null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Cart.UserOrSessionRequired");
    }

    [Fact]
    public void Create_WithBothUserIdAndSessionIdEmpty_ShouldFail()
    {
        var result = Cart.Create(_tenantId, userId: null, sessionId: "   ");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Cart.UserOrSessionRequired");
    }

    [Fact]
    public void Create_ShouldRaiseCartCreatedDomainEvent()
    {
        var result = Cart.Create(_tenantId, userId: _userId);

        result.IsSuccess.Should().BeTrue();
        var cart = result.Value;
        cart.DomainEvents.Should().HaveCount(1);
        var domainEvent = cart.DomainEvents.First();
        domainEvent.Should().BeOfType<IDelivery.Domain.Carts.Events.CartCreatedDomainEvent>();
        var evt = (IDelivery.Domain.Carts.Events.CartCreatedDomainEvent)domainEvent;
        evt.CartId.Should().Be(cart.Id);
        evt.TenantId.Should().Be(_tenantId);
        evt.UserId.Should().Be(_userId);
    }

    #endregion

    #region Cart.AddItem

    [Fact]
    public void AddItem_NewItem_ShouldAddToCart()
    {
        var cart = Cart.Create(_tenantId, userId: _userId).Value;

        var result = cart.AddItem(_productId, "Hamburger", _price, 2);

        result.IsSuccess.Should().BeTrue();
        cart.Items.Should().HaveCount(1);
        cart.Items.First().ProductId.Should().Be(_productId);
        cart.Items.First().ProductName.Should().Be("Hamburger");
        cart.Items.First().Quantity.Should().Be(2);
        cart.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void AddItem_ExistingProduct_ShouldIncreaseQuantity()
    {
        var cart = Cart.Create(_tenantId, userId: _userId).Value;
        cart.AddItem(_productId, "Hamburger", _price, 2);

        var result = cart.AddItem(_productId, "Hamburger", _price, 3);

        result.IsSuccess.Should().BeTrue();
        cart.Items.Should().HaveCount(1);
        cart.Items.First().Quantity.Should().Be(5);
    }

    [Fact]
    public void AddItem_ExistingProduct_CurrencyMismatch_ShouldFail()
    {
        var cart = Cart.Create(_tenantId, userId: _userId).Value;
        cart.AddItem(_productId, "Hamburger", _price, 2);

        var usdPrice = Money.Create(10.00m, "USD").Value;
        var result = cart.AddItem(_productId, "Hamburger", usdPrice, 1);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Cart.CurrencyMismatch");
        cart.Items.First().Quantity.Should().Be(2);
    }

    [Fact]
    public void AddItem_WithZeroQuantity_ShouldFail()
    {
        var cart = Cart.Create(_tenantId, userId: _userId).Value;

        var result = cart.AddItem(_productId, "Hamburger", _price, 0);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Cart.InvalidQuantity");
    }

    [Fact]
    public void AddItem_WithNegativeQuantity_ShouldFail()
    {
        var cart = Cart.Create(_tenantId, userId: _userId).Value;

        var result = cart.AddItem(_productId, "Hamburger", _price, -1);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Cart.InvalidQuantity");
    }

    [Fact]
    public void AddItem_WithEmptyProductId_ShouldFail()
    {
        var cart = Cart.Create(_tenantId, userId: _userId).Value;

        var result = cart.AddItem(Guid.Empty, "Hamburger", _price);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Cart.ProductRequired");
    }

    [Fact]
    public void AddItem_WithEmptyProductName_ShouldFail()
    {
        var cart = Cart.Create(_tenantId, userId: _userId).Value;

        var result = cart.AddItem(_productId, "", _price);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Cart.ProductNameRequired");
    }

    [Fact]
    public void AddItem_WithNegativePrice_ShouldFail()
    {
        var cart = Cart.Create(_tenantId, userId: _userId).Value;
        var negativePrice = CreateNegativeMoney(-5.00m, "BRL");

        var result = cart.AddItem(_productId, "Hamburger", negativePrice);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Cart.InvalidPrice");
    }

    [Fact]
    public void AddItem_ShouldRaiseCartItemAddedDomainEvent()
    {
        var cart = Cart.Create(_tenantId, userId: _userId).Value;
        cart.ClearDomainEvents();

        cart.AddItem(_productId, "Hamburger", _price, 2);

        var addedEvent = cart.DomainEvents
            .OfType<IDelivery.Domain.Carts.Events.CartItemAddedDomainEvent>()
            .FirstOrDefault();
        addedEvent.Should().NotBeNull();
        addedEvent!.ProductId.Should().Be(_productId);
        addedEvent.Quantity.Should().Be(2);
        addedEvent.TenantId.Should().Be(_tenantId);
    }

    #endregion

    #region Cart.RemoveItem

    [Fact]
    public void RemoveItem_ExistingItem_ShouldRemove()
    {
        var cart = Cart.Create(_tenantId, userId: _userId).Value;
        cart.AddItem(_productId, "Hamburger", _price, 2);

        var result = cart.RemoveItem(_productId);

        result.IsSuccess.Should().BeTrue();
        cart.Items.Should().BeEmpty();
        cart.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void RemoveItem_NonExistentItem_ShouldFail()
    {
        var cart = Cart.Create(_tenantId, userId: _userId).Value;

        var result = cart.RemoveItem(Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Cart.ItemNotFound");
    }

    [Fact]
    public void RemoveItem_ShouldRaiseCartItemRemovedDomainEvent()
    {
        var cart = Cart.Create(_tenantId, userId: _userId).Value;
        cart.AddItem(_productId, "Hamburger", _price, 2);
        cart.ClearDomainEvents();

        cart.RemoveItem(_productId);

        var removedEvent = cart.DomainEvents
            .OfType<IDelivery.Domain.Carts.Events.CartItemRemovedDomainEvent>()
            .FirstOrDefault();
        removedEvent.Should().NotBeNull();
        removedEvent!.ProductId.Should().Be(_productId);
        removedEvent.TenantId.Should().Be(_tenantId);
    }

    #endregion

    #region Cart.UpdateItemQuantity

    [Fact]
    public void UpdateItemQuantity_ExistingItem_ShouldUpdate()
    {
        var cart = Cart.Create(_tenantId, userId: _userId).Value;
        cart.AddItem(_productId, "Hamburger", _price, 2);

        var result = cart.UpdateItemQuantity(_productId, 10);

        result.IsSuccess.Should().BeTrue();
        cart.Items.First().Quantity.Should().Be(10);
        cart.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdateItemQuantity_WithZeroQuantity_ShouldFail()
    {
        var cart = Cart.Create(_tenantId, userId: _userId).Value;
        cart.AddItem(_productId, "Hamburger", _price, 2);

        var result = cart.UpdateItemQuantity(_productId, 0);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Cart.InvalidQuantity");
    }

    [Fact]
    public void UpdateItemQuantity_WithNegativeQuantity_ShouldFail()
    {
        var cart = Cart.Create(_tenantId, userId: _userId).Value;
        cart.AddItem(_productId, "Hamburger", _price, 2);

        var result = cart.UpdateItemQuantity(_productId, -5);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Cart.InvalidQuantity");
    }

    [Fact]
    public void UpdateItemQuantity_NonExistentItem_ShouldFail()
    {
        var cart = Cart.Create(_tenantId, userId: _userId).Value;

        var result = cart.UpdateItemQuantity(Guid.NewGuid(), 5);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Cart.ItemNotFound");
    }

    #endregion

    #region Cart.Clear

    [Fact]
    public void Clear_WithItems_ShouldClearAllItems()
    {
        var cart = Cart.Create(_tenantId, userId: _userId).Value;
        cart.AddItem(_productId, "Hamburger", _price, 2);
        var anotherProductId = Guid.NewGuid();
        cart.AddItem(anotherProductId, "Fries", Money.Create(10.00m, "BRL").Value, 1);

        var result = cart.Clear();

        result.IsSuccess.Should().BeTrue();
        cart.Items.Should().BeEmpty();
        cart.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Clear_WhenAlreadyEmpty_ShouldFail()
    {
        var cart = Cart.Create(_tenantId, userId: _userId).Value;

        var result = cart.Clear();

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Cart.AlreadyEmpty");
    }

    [Fact]
    public void Clear_ShouldRaiseCartClearedDomainEvent()
    {
        var cart = Cart.Create(_tenantId, userId: _userId).Value;
        cart.AddItem(_productId, "Hamburger", _price, 1);
        cart.ClearDomainEvents();

        cart.Clear();

        var clearedEvent = cart.DomainEvents
            .OfType<IDelivery.Domain.Carts.Events.CartClearedDomainEvent>()
            .FirstOrDefault();
        clearedEvent.Should().NotBeNull();
        clearedEvent!.CartId.Should().Be(cart.Id);
        clearedEvent.TenantId.Should().Be(_tenantId);
    }

    #endregion

    #region Cart.GetTotal

    [Fact]
    public void GetTotal_WithItems_ShouldReturnSumOfSubtotals()
    {
        var cart = Cart.Create(_tenantId, userId: _userId).Value;
        cart.AddItem(_productId, "Hamburger", _price, 2);
        var friesId = Guid.NewGuid();
        cart.AddItem(friesId, "Fries", Money.Create(10.00m, "BRL").Value, 1);

        var total = cart.GetTotal();

        total.Amount.Should().Be(61.00m);
        total.Currency.Should().Be("BRL");
    }

    [Fact]
    public void GetTotal_EmptyCart_ShouldReturnZero()
    {
        var cart = Cart.Create(_tenantId, userId: _userId).Value;

        var total = cart.GetTotal();

        total.Amount.Should().Be(0m);
        total.Currency.Should().Be("BRL");
    }

    #endregion

    #region Cart.GetItemCount

    [Fact]
    public void GetItemCount_WithItems_ShouldReturnTotalQuantity()
    {
        var cart = Cart.Create(_tenantId, userId: _userId).Value;
        cart.AddItem(_productId, "Hamburger", _price, 2);
        var friesId = Guid.NewGuid();
        cart.AddItem(friesId, "Fries", Money.Create(10.00m, "BRL").Value, 3);

        var count = cart.GetItemCount();

        count.Should().Be(5);
    }

    [Fact]
    public void GetItemCount_EmptyCart_ShouldReturnZero()
    {
        var cart = Cart.Create(_tenantId, userId: _userId).Value;

        var count = cart.GetItemCount();

        count.Should().Be(0);
    }

    #endregion

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

public class CartItemTests
{
    private readonly Guid _cartId = Guid.NewGuid();
    private readonly Guid _productId = Guid.NewGuid();
    private readonly Money _price = Money.Create(15.00m, "BRL").Value;

    #region CartItem.Create

    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        var result = CartItem.Create(_cartId, _productId, "Cola", _price, 3);

        result.IsSuccess.Should().BeTrue();
        var item = result.Value;
        item.Id.Should().NotBeEmpty();
        item.CartId.Should().Be(_cartId);
        item.ProductId.Should().Be(_productId);
        item.ProductName.Should().Be("Cola");
        item.Quantity.Should().Be(3);
        item.UnitPrice.Amount.Should().Be(15.00m);
        item.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_WithEmptyCartId_ShouldFail()
    {
        var result = CartItem.Create(Guid.Empty, _productId, "Cola", _price);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CartItem.CartRequired");
    }

    [Fact]
    public void Create_WithEmptyProductId_ShouldFail()
    {
        var result = CartItem.Create(_cartId, Guid.Empty, "Cola", _price);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CartItem.ProductRequired");
    }

    [Fact]
    public void Create_WithEmptyProductName_ShouldFail()
    {
        var result = CartItem.Create(_cartId, _productId, "", _price);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CartItem.ProductNameRequired");
    }

    [Fact]
    public void Create_WithNegativePrice_ShouldFail()
    {
        var negativePrice = CreateNegativeMoney(-5.00m, "BRL");

        var result = CartItem.Create(_cartId, _productId, "Cola", negativePrice);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CartItem.InvalidPrice");
    }

    [Fact]
    public void Create_WithZeroQuantity_ShouldFail()
    {
        var result = CartItem.Create(_cartId, _productId, "Cola", _price, 0);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CartItem.InvalidQuantity");
    }

    [Fact]
    public void Create_WithNegativeQuantity_ShouldFail()
    {
        var result = CartItem.Create(_cartId, _productId, "Cola", _price, -1);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CartItem.InvalidQuantity");
    }

    #endregion

    #region CartItem.IncreaseQuantity

    [Fact]
    public void IncreaseQuantity_WithPositiveAmount_ShouldIncrease()
    {
        var item = CartItem.Create(_cartId, _productId, "Cola", _price, 2).Value;

        item.IncreaseQuantity(3);

        item.Quantity.Should().Be(5);
        item.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void IncreaseQuantity_WithZero_ShouldIgnore()
    {
        var item = CartItem.Create(_cartId, _productId, "Cola", _price, 2).Value;

        item.IncreaseQuantity(0);

        item.Quantity.Should().Be(2);
    }

    [Fact]
    public void IncreaseQuantity_WithNegative_ShouldIgnore()
    {
        var item = CartItem.Create(_cartId, _productId, "Cola", _price, 2).Value;

        item.IncreaseQuantity(-3);

        item.Quantity.Should().Be(2);
    }

    #endregion

    #region CartItem.UpdateQuantity

    [Fact]
    public void UpdateQuantity_WithPositiveValue_ShouldUpdate()
    {
        var item = CartItem.Create(_cartId, _productId, "Cola", _price, 2).Value;

        item.UpdateQuantity(10);

        item.Quantity.Should().Be(10);
        item.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdateQuantity_WithZero_ShouldIgnore()
    {
        var item = CartItem.Create(_cartId, _productId, "Cola", _price, 5).Value;

        item.UpdateQuantity(0);

        item.Quantity.Should().Be(5);
    }

    [Fact]
    public void UpdateQuantity_WithNegative_ShouldIgnore()
    {
        var item = CartItem.Create(_cartId, _productId, "Cola", _price, 5).Value;

        item.UpdateQuantity(-1);

        item.Quantity.Should().Be(5);
    }

    #endregion

    #region CartItem.Subtotal

    [Fact]
    public void Subtotal_ShouldReturnPriceTimesQuantity()
    {
        var item = CartItem.Create(_cartId, _productId, "Cola", _price, 4).Value;

        var subtotal = item.Subtotal;

        subtotal.Amount.Should().Be(60.00m);
        subtotal.Currency.Should().Be("BRL");
    }

    #endregion

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
