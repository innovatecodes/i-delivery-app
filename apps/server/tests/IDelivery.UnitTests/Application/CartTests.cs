using IDelivery.Application.Commands.Carts;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.Application.Abstractions.Authentication;
using IDelivery.Domain.Carts.Entities;
using IDelivery.Domain.Common.ValueObjects;
using FluentAssertions;
using Moq;
using System;
using System.Threading;
using Xunit;

namespace IDelivery.UnitTests.Application;

public class AddCartItemCommandHandlerTests
{
    private readonly Mock<ICartRepository> _mockCartRepository;
    private readonly Mock<ITenantContext> _mockTenantContext;
    private readonly Mock<ICurrentUser> _mockCurrentUser;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    public AddCartItemCommandHandlerTests()
    {
        _mockCartRepository = new Mock<ICartRepository>();
        _mockTenantContext = new Mock<ITenantContext>();
        _mockCurrentUser = new Mock<ICurrentUser>();
        _mockTenantContext.Setup(x => x.TenantId).Returns(_tenantId);
        _mockCurrentUser.Setup(x => x.UserId).Returns(_userId);
    }

    [Fact]
    public async Task Handle_WhenCartExists_ShouldAddItemToExistingCart()
    {
        var cartResult = Cart.Create(_tenantId, _userId);
        Assert.True(cartResult.IsSuccess);
        var cart = cartResult.Value;

        _mockCartRepository.Setup(x => x.GetByUserIdAsync(_tenantId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        var command = new AddCartItemCommand(
            Guid.NewGuid(),
            "Coca-Cola 350ml",
            5.50m,
            "BRL",
            2);

        var handler = new AddCartItemCommandHandler(
            _mockCartRepository.Object,
            _mockTenantContext.Object,
            _mockCurrentUser.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        cart.Items.Should().HaveCount(1);
        cart.Items[0].Quantity.Should().Be(2);
        _mockCartRepository.Verify(x => x.UpdateAsync(cart, It.IsAny<CancellationToken>()), Times.Once);
        _mockCartRepository.Verify(x => x.AddAsync(It.IsAny<Cart>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCartDoesNotExist_ShouldCreateCartThenAddItem()
    {
        _mockCartRepository.Setup(x => x.GetByUserIdAsync(_tenantId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cart?)null);

        var command = new AddCartItemCommand(
            Guid.NewGuid(),
            "Coca-Cola 350ml",
            5.50m,
            "BRL",
            1);

        var handler = new AddCartItemCommandHandler(
            _mockCartRepository.Object,
            _mockTenantContext.Object,
            _mockCurrentUser.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _mockCartRepository.Verify(x => x.AddAsync(It.IsAny<Cart>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockCartRepository.Verify(x => x.UpdateAsync(It.IsAny<Cart>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithoutTenant_ShouldFail()
    {
        _mockTenantContext.Setup(x => x.TenantId).Returns((Guid?)null);

        var command = new AddCartItemCommand(
            Guid.NewGuid(),
            "Coca-Cola 350ml",
            5.50m,
            "BRL",
            1);

        var handler = new AddCartItemCommandHandler(
            _mockCartRepository.Object,
            _mockTenantContext.Object,
            _mockCurrentUser.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Cart.TenantRequired");
    }

    [Fact]
    public async Task Handle_WithoutUser_ShouldFail()
    {
        _mockCurrentUser.Setup(x => x.UserId).Returns((Guid?)null);

        var command = new AddCartItemCommand(
            Guid.NewGuid(),
            "Coca-Cola 350ml",
            5.50m,
            "BRL",
            1);

        var handler = new AddCartItemCommandHandler(
            _mockCartRepository.Object,
            _mockTenantContext.Object,
            _mockCurrentUser.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Cart.UserRequired");
    }

    [Fact]
    public async Task Handle_WithNegativePrice_ShouldFail()
    {
        _mockCartRepository.Setup(x => x.GetByUserIdAsync(_tenantId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cart?)null);

        var command = new AddCartItemCommand(
            Guid.NewGuid(),
            "Coca-Cola 350ml",
            -1.00m,
            "BRL",
            1);

        var handler = new AddCartItemCommandHandler(
            _mockCartRepository.Object,
            _mockTenantContext.Object,
            _mockCurrentUser.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Money.NegativeAmount");
    }

    [Fact]
    public async Task Handle_WithEmptyProductName_ShouldFail()
    {
        var productId = Guid.NewGuid();
        var cartResult = Cart.Create(_tenantId, _userId);
        Assert.True(cartResult.IsSuccess);
        var cart = cartResult.Value;

        _mockCartRepository.Setup(x => x.GetByUserIdAsync(_tenantId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        var command = new AddCartItemCommand(
            productId,
            "",
            5.50m,
            "BRL",
            1);

        var handler = new AddCartItemCommandHandler(
            _mockCartRepository.Object,
            _mockTenantContext.Object,
            _mockCurrentUser.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Cart.ProductNameRequired");
    }
}

public class RemoveCartItemCommandHandlerTests
{
    private readonly Mock<ICartRepository> _mockCartRepository;
    private readonly Mock<ITenantContext> _mockTenantContext;
    private readonly Mock<ICurrentUser> _mockCurrentUser;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    public RemoveCartItemCommandHandlerTests()
    {
        _mockCartRepository = new Mock<ICartRepository>();
        _mockTenantContext = new Mock<ITenantContext>();
        _mockCurrentUser = new Mock<ICurrentUser>();
        _mockTenantContext.Setup(x => x.TenantId).Returns(_tenantId);
        _mockCurrentUser.Setup(x => x.UserId).Returns(_userId);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldRemoveItem()
    {
        var productId = Guid.NewGuid();
        var cartResult = Cart.Create(_tenantId, _userId);
        Assert.True(cartResult.IsSuccess);
        var cart = cartResult.Value;

        var addItemResult = cart.AddItem(productId, "Coca-Cola", Money.Create(5.50m, "BRL").Value, 2);
        Assert.True(addItemResult.IsSuccess);

        _mockCartRepository.Setup(x => x.GetByUserIdAsync(_tenantId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        var command = new RemoveCartItemCommand(productId);

        var handler = new RemoveCartItemCommandHandler(
            _mockCartRepository.Object,
            _mockTenantContext.Object,
            _mockCurrentUser.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        cart.Items.Should().BeEmpty();
        _mockCartRepository.Verify(x => x.UpdateAsync(cart, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithoutTenant_ShouldFail()
    {
        _mockTenantContext.Setup(x => x.TenantId).Returns((Guid?)null);

        var command = new RemoveCartItemCommand(Guid.NewGuid());

        var handler = new RemoveCartItemCommandHandler(
            _mockCartRepository.Object,
            _mockTenantContext.Object,
            _mockCurrentUser.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Cart.TenantRequired");
    }

    [Fact]
    public async Task Handle_WithoutUser_ShouldFail()
    {
        _mockCurrentUser.Setup(x => x.UserId).Returns((Guid?)null);

        var command = new RemoveCartItemCommand(Guid.NewGuid());

        var handler = new RemoveCartItemCommandHandler(
            _mockCartRepository.Object,
            _mockTenantContext.Object,
            _mockCurrentUser.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Cart.UserRequired");
    }

    [Fact]
    public async Task Handle_WhenCartNotFound_ShouldFail()
    {
        _mockCartRepository.Setup(x => x.GetByUserIdAsync(_tenantId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cart?)null);

        var command = new RemoveCartItemCommand(Guid.NewGuid());

        var handler = new RemoveCartItemCommandHandler(
            _mockCartRepository.Object,
            _mockTenantContext.Object,
            _mockCurrentUser.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Cart.NotFound");
    }

    [Fact]
    public async Task Handle_WhenItemNotInCart_ShouldFail()
    {
        var productId = Guid.NewGuid();
        var cartResult = Cart.Create(_tenantId, _userId);
        Assert.True(cartResult.IsSuccess);
        var cart = cartResult.Value;

        var addItemResult = cart.AddItem(productId, "Coca-Cola", Money.Create(5.50m, "BRL").Value, 1);
        Assert.True(addItemResult.IsSuccess);

        _mockCartRepository.Setup(x => x.GetByUserIdAsync(_tenantId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        var command = new RemoveCartItemCommand(Guid.NewGuid());

        var handler = new RemoveCartItemCommandHandler(
            _mockCartRepository.Object,
            _mockTenantContext.Object,
            _mockCurrentUser.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Cart.ItemNotFound");
    }
}

public class UpdateCartItemQuantityCommandHandlerTests
{
    private readonly Mock<ICartRepository> _mockCartRepository;
    private readonly Mock<ITenantContext> _mockTenantContext;
    private readonly Mock<ICurrentUser> _mockCurrentUser;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    public UpdateCartItemQuantityCommandHandlerTests()
    {
        _mockCartRepository = new Mock<ICartRepository>();
        _mockTenantContext = new Mock<ITenantContext>();
        _mockCurrentUser = new Mock<ICurrentUser>();
        _mockTenantContext.Setup(x => x.TenantId).Returns(_tenantId);
        _mockCurrentUser.Setup(x => x.UserId).Returns(_userId);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateQuantity()
    {
        var productId = Guid.NewGuid();
        var cartResult = Cart.Create(_tenantId, _userId);
        Assert.True(cartResult.IsSuccess);
        var cart = cartResult.Value;

        var addItemResult = cart.AddItem(productId, "Coca-Cola", Money.Create(5.50m, "BRL").Value, 1);
        Assert.True(addItemResult.IsSuccess);

        _mockCartRepository.Setup(x => x.GetByUserIdAsync(_tenantId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        var command = new UpdateCartItemQuantityCommand(productId, 5);

        var handler = new UpdateCartItemQuantityCommandHandler(
            _mockCartRepository.Object,
            _mockTenantContext.Object,
            _mockCurrentUser.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        cart.Items[0].Quantity.Should().Be(5);
        _mockCartRepository.Verify(x => x.UpdateAsync(cart, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithoutTenant_ShouldFail()
    {
        _mockTenantContext.Setup(x => x.TenantId).Returns((Guid?)null);

        var command = new UpdateCartItemQuantityCommand(Guid.NewGuid(), 2);

        var handler = new UpdateCartItemQuantityCommandHandler(
            _mockCartRepository.Object,
            _mockTenantContext.Object,
            _mockCurrentUser.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Cart.TenantRequired");
    }

    [Fact]
    public async Task Handle_WithoutUser_ShouldFail()
    {
        _mockCurrentUser.Setup(x => x.UserId).Returns((Guid?)null);

        var command = new UpdateCartItemQuantityCommand(Guid.NewGuid(), 2);

        var handler = new UpdateCartItemQuantityCommandHandler(
            _mockCartRepository.Object,
            _mockTenantContext.Object,
            _mockCurrentUser.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Cart.UserRequired");
    }

    [Fact]
    public async Task Handle_WhenCartNotFound_ShouldFail()
    {
        _mockCartRepository.Setup(x => x.GetByUserIdAsync(_tenantId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cart?)null);

        var command = new UpdateCartItemQuantityCommand(Guid.NewGuid(), 2);

        var handler = new UpdateCartItemQuantityCommandHandler(
            _mockCartRepository.Object,
            _mockTenantContext.Object,
            _mockCurrentUser.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Cart.NotFound");
    }

    [Fact]
    public async Task Handle_WhenItemNotInCart_ShouldFail()
    {
        var productId = Guid.NewGuid();
        var cartResult = Cart.Create(_tenantId, _userId);
        Assert.True(cartResult.IsSuccess);
        var cart = cartResult.Value;

        var addItemResult = cart.AddItem(productId, "Coca-Cola", Money.Create(5.50m, "BRL").Value, 1);
        Assert.True(addItemResult.IsSuccess);

        _mockCartRepository.Setup(x => x.GetByUserIdAsync(_tenantId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        var command = new UpdateCartItemQuantityCommand(Guid.NewGuid(), 2);

        var handler = new UpdateCartItemQuantityCommandHandler(
            _mockCartRepository.Object,
            _mockTenantContext.Object,
            _mockCurrentUser.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Cart.ItemNotFound");
    }

    [Fact]
    public async Task Handle_WithInvalidQuantity_ShouldFail()
    {
        var productId = Guid.NewGuid();
        var cartResult = Cart.Create(_tenantId, _userId);
        Assert.True(cartResult.IsSuccess);
        var cart = cartResult.Value;

        var addItemResult = cart.AddItem(productId, "Coca-Cola", Money.Create(5.50m, "BRL").Value, 1);
        Assert.True(addItemResult.IsSuccess);

        _mockCartRepository.Setup(x => x.GetByUserIdAsync(_tenantId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        var command = new UpdateCartItemQuantityCommand(productId, 0);

        var handler = new UpdateCartItemQuantityCommandHandler(
            _mockCartRepository.Object,
            _mockTenantContext.Object,
            _mockCurrentUser.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Cart.InvalidQuantity");
    }
}

public class ClearCartCommandHandlerTests
{
    private readonly Mock<ICartRepository> _mockCartRepository;
    private readonly Mock<ITenantContext> _mockTenantContext;
    private readonly Mock<ICurrentUser> _mockCurrentUser;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    public ClearCartCommandHandlerTests()
    {
        _mockCartRepository = new Mock<ICartRepository>();
        _mockTenantContext = new Mock<ITenantContext>();
        _mockCurrentUser = new Mock<ICurrentUser>();
        _mockTenantContext.Setup(x => x.TenantId).Returns(_tenantId);
        _mockCurrentUser.Setup(x => x.UserId).Returns(_userId);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldClearCart()
    {
        var productId = Guid.NewGuid();
        var cartResult = Cart.Create(_tenantId, _userId);
        Assert.True(cartResult.IsSuccess);
        var cart = cartResult.Value;

        var addItemResult = cart.AddItem(productId, "Coca-Cola", Money.Create(5.50m, "BRL").Value, 2);
        Assert.True(addItemResult.IsSuccess);

        _mockCartRepository.Setup(x => x.GetByUserIdAsync(_tenantId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        var command = new ClearCartCommand();

        var handler = new ClearCartCommandHandler(
            _mockCartRepository.Object,
            _mockTenantContext.Object,
            _mockCurrentUser.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        cart.Items.Should().BeEmpty();
        _mockCartRepository.Verify(x => x.UpdateAsync(cart, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithoutTenant_ShouldFail()
    {
        _mockTenantContext.Setup(x => x.TenantId).Returns((Guid?)null);

        var command = new ClearCartCommand();

        var handler = new ClearCartCommandHandler(
            _mockCartRepository.Object,
            _mockTenantContext.Object,
            _mockCurrentUser.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Cart.TenantRequired");
    }

    [Fact]
    public async Task Handle_WithoutUser_ShouldFail()
    {
        _mockCurrentUser.Setup(x => x.UserId).Returns((Guid?)null);

        var command = new ClearCartCommand();

        var handler = new ClearCartCommandHandler(
            _mockCartRepository.Object,
            _mockTenantContext.Object,
            _mockCurrentUser.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Cart.UserRequired");
    }

    [Fact]
    public async Task Handle_WhenCartNotFound_ShouldFail()
    {
        _mockCartRepository.Setup(x => x.GetByUserIdAsync(_tenantId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cart?)null);

        var command = new ClearCartCommand();

        var handler = new ClearCartCommandHandler(
            _mockCartRepository.Object,
            _mockTenantContext.Object,
            _mockCurrentUser.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Cart.NotFound");
    }

    [Fact]
    public async Task Handle_WhenCartAlreadyEmpty_ShouldFail()
    {
        var cartResult = Cart.Create(_tenantId, _userId);
        Assert.True(cartResult.IsSuccess);
        var cart = cartResult.Value;

        _mockCartRepository.Setup(x => x.GetByUserIdAsync(_tenantId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        var command = new ClearCartCommand();

        var handler = new ClearCartCommandHandler(
            _mockCartRepository.Object,
            _mockTenantContext.Object,
            _mockCurrentUser.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Cart.AlreadyEmpty");
    }
}
