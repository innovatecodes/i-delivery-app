using System.Reflection;
using FluentAssertions;
using IDelivery.Domain.Common.ValueObjects;
using IDelivery.Domain.Orders.Entities;
using IDelivery.Domain.Orders.Enums;
using IDelivery.Domain.Orders.Events;
using Xunit;

namespace IDelivery.UnitTests.Domain;

public class OrderTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _customerId = Guid.NewGuid();
    private readonly Guid _driverId = Guid.NewGuid();

    private static readonly Address ValidAddress = new(
        "Rua Augusta",
        "1234",
        null,
        "Consolação",
        "São Paulo",
        "SP",
        ZipCode.Create("01310-100").Value);

    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        var items = new List<OrderItem> { CreateOrderItem(15.00m, 2) };

        var result = Order.Create(
            _tenantId,
            _customerId,
            items,
            Money.Create(5.00m).Value,
            ValidAddress);

        result.IsSuccess.Should().BeTrue();
        var order = result.Value;
        order.Id.Should().NotBeEmpty();
        order.TenantId.Should().Be(_tenantId);
        order.CustomerId.Should().Be(_customerId);
        order.State.Should().Be(OrderState.Pending);
        order.Items.Should().HaveCount(1);
        order.Subtotal.Amount.Should().Be(30.00m);
        order.DeliveryFee.Amount.Should().Be(5.00m);
        order.TotalAmount.Amount.Should().Be(35.00m);
        order.DeliveryAddress.Should().Be(ValidAddress);
    }

    [Fact]
    public void Create_ShouldDispatchOrderCreatedEvent()
    {
        var items = new List<OrderItem> { CreateOrderItem(10.00m, 1) };

        var result = Order.Create(
            _tenantId,
            _customerId,
            items,
            Money.Create(5.00m).Value,
            ValidAddress);

        result.Value.DomainEvents.Should().Contain(e => e is OrderCreatedDomainEvent);
    }

    [Fact]
    public void Create_WithEmptyTenantId_ShouldFail()
    {
        var items = new List<OrderItem> { CreateOrderItem(10.00m, 1) };

        var result = Order.Create(
            Guid.Empty,
            _customerId,
            items,
            Money.Create(5.00m).Value,
            ValidAddress);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.TenantRequired");
    }

    [Fact]
    public void Create_WithEmptyCustomerId_ShouldFail()
    {
        var items = new List<OrderItem> { CreateOrderItem(10.00m, 1) };

        var result = Order.Create(
            _tenantId,
            Guid.Empty,
            items,
            Money.Create(5.00m).Value,
            ValidAddress);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.CustomerRequired");
    }

    [Fact]
    public void Create_WithEmptyItems_ShouldFail()
    {
        var result = Order.Create(
            _tenantId,
            _customerId,
            new List<OrderItem>(),
            Money.Create(5.00m).Value,
            ValidAddress);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.ItemsRequired");
    }

    [Fact]
    public void Create_WithNullItems_ShouldFail()
    {
        var result = Order.Create(
            _tenantId,
            _customerId,
            null!,
            Money.Create(5.00m).Value,
            ValidAddress);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.ItemsRequired");
    }

    [Fact]
    public void Create_WithNegativeDeliveryFee_ShouldFail()
    {
        var items = new List<OrderItem> { CreateOrderItem(10.00m, 1) };

        var result = Order.Create(
            _tenantId,
            _customerId,
            items,
            CreateNegativeMoney(-1.00m),
            ValidAddress);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.InvalidDeliveryFee");
    }

    [Fact]
    public void Create_WithNullAddress_ShouldFail()
    {
        var items = new List<OrderItem> { CreateOrderItem(10.00m, 1) };

        var result = Order.Create(
            _tenantId,
            _customerId,
            items,
            Money.Create(5.00m).Value,
            null!);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.AddressRequired");
    }

    [Fact]
    public void Confirm_WhenPending_ShouldSucceed()
    {
        var order = CreatePendingOrder();

        var result = order.Confirm();

        result.IsSuccess.Should().BeTrue();
        order.State.Should().Be(OrderState.Confirmed);
        order.ConfirmedAt.Should().NotBeNull();
    }

    [Fact]
    public void Confirm_WhenPending_ShouldDispatchStatusChangedEvent()
    {
        var order = CreatePendingOrder();

        order.Confirm();

        order.DomainEvents.Should().Contain(e => e is OrderStatusChangedDomainEvent);
        order.DomainEvents.OfType<OrderStatusChangedDomainEvent>().Should()
            .Contain(e => e.FromState == OrderState.Pending && e.ToState == OrderState.Confirmed);
    }

    [Fact]
    public void Confirm_WhenAlreadyConfirmed_ShouldFail()
    {
        var order = CreatePendingOrder();
        order.Confirm();

        var result = order.Confirm();

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.InvalidTransition");
    }

    [Fact]
    public void StartPreparing_WhenConfirmed_ShouldSucceed()
    {
        var order = CreateConfirmedOrder();

        var result = order.StartPreparing();

        result.IsSuccess.Should().BeTrue();
        order.State.Should().Be(OrderState.Preparing);
        order.PreparingAt.Should().NotBeNull();
    }

    [Fact]
    public void StartPreparing_WhenPending_ShouldFail()
    {
        var order = CreatePendingOrder();

        var result = order.StartPreparing();

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.InvalidTransition");
    }

    [Fact]
    public void MarkReadyForDelivery_WhenPreparing_ShouldSucceed()
    {
        var order = CreatePreparingOrder();

        var result = order.MarkReadyForDelivery();

        result.IsSuccess.Should().BeTrue();
        order.State.Should().Be(OrderState.ReadyForDelivery);
        order.ReadyAt.Should().NotBeNull();
    }

    [Fact]
    public void MarkReadyForDelivery_WhenConfirmed_ShouldFail()
    {
        var order = CreateConfirmedOrder();

        var result = order.MarkReadyForDelivery();

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.InvalidTransition");
    }

    [Fact]
    public void StartDelivery_WhenReadyForDelivery_ShouldSucceed()
    {
        var order = CreateReadyForDeliveryOrder();

        var result = order.StartDelivery(_driverId);

        result.IsSuccess.Should().BeTrue();
        order.State.Should().Be(OrderState.OutForDelivery);
        order.DeliveryDriverId.Should().Be(_driverId);
        order.OutForDeliveryAt.Should().NotBeNull();
    }

    [Fact]
    public void StartDelivery_WhenPreparing_ShouldFail()
    {
        var order = CreatePreparingOrder();

        var result = order.StartDelivery(_driverId);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.InvalidTransition");
    }

    [Fact]
    public void StartDelivery_WithEmptyDriverId_ShouldFail()
    {
        var order = CreateReadyForDeliveryOrder();

        var result = order.StartDelivery(Guid.Empty);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.DriverRequired");
    }

    [Fact]
    public void Deliver_WhenOutForDeliveryWithMatchingDriver_ShouldSucceed()
    {
        var order = CreateOutForDeliveryOrder();

        var result = order.Deliver(_driverId);

        result.IsSuccess.Should().BeTrue();
        order.State.Should().Be(OrderState.Delivered);
        order.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public void Deliver_WhenOutForDelivery_ShouldDispatchEvents()
    {
        var order = CreateOutForDeliveryOrder();

        order.Deliver(_driverId);

        order.DomainEvents.OfType<OrderStatusChangedDomainEvent>().Should()
            .Contain(e => e.FromState == OrderState.OutForDelivery && e.ToState == OrderState.Delivered);
        order.DomainEvents.Should().Contain(e => e is OrderDeliveredDomainEvent);
    }

    [Fact]
    public void Deliver_WhenPreparing_ShouldFail()
    {
        var order = CreatePreparingOrder();

        var result = order.Deliver(_driverId);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.InvalidTransition");
    }

    [Fact]
    public void Deliver_WithWrongDriverId_ShouldFail()
    {
        var order = CreateOutForDeliveryOrder();

        var result = order.Deliver(Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.UnauthorizedDriver");
    }

    [Fact]
    public void FailDelivery_WhenOutForDelivery_ShouldSucceed()
    {
        var order = CreateOutForDeliveryOrder();

        var result = order.FailDelivery(_driverId, DeliveryFailureReason.CustomerAbsent);

        result.IsSuccess.Should().BeTrue();
        order.State.Should().Be(OrderState.DeliveryFailed);
        order.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public void FailDelivery_WithWrongDriverId_ShouldFail()
    {
        var order = CreateOutForDeliveryOrder();

        var result = order.FailDelivery(Guid.NewGuid(), DeliveryFailureReason.CustomerAbsent);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.UnauthorizedDriver");
    }

    [Fact]
    public void FailDelivery_WithOtherReasonAndNoDetail_ShouldFail()
    {
        var order = CreateOutForDeliveryOrder();

        var result = order.FailDelivery(_driverId, DeliveryFailureReason.Other, null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.FailureReasonDetailRequired");
    }

    [Fact]
    public void FailDelivery_WithOtherReasonAndDetail_ShouldSucceed()
    {
        var order = CreateOutForDeliveryOrder();

        var result = order.FailDelivery(
            _driverId,
            DeliveryFailureReason.Other,
            "Portão trancado");

        result.IsSuccess.Should().BeTrue();
        order.State.Should().Be(OrderState.DeliveryFailed);
        order.DeliveryFailureReasonDetail.Should().Be("Portão trancado");
    }

    [Fact]
    public void FailDelivery_WhenPreparing_ShouldFail()
    {
        var order = CreatePreparingOrder();

        var result = order.FailDelivery(_driverId, DeliveryFailureReason.CustomerAbsent);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.InvalidTransition");
    }

    [Fact]
    public void Cancel_WhenPending_ShouldSucceed()
    {
        var order = CreatePendingOrder();

        var result = order.Cancel("customer");

        result.IsSuccess.Should().BeTrue();
        order.State.Should().Be(OrderState.Cancelled);
        order.CancelledAt.Should().NotBeNull();
    }

    [Fact]
    public void Cancel_WhenConfirmed_ShouldSucceed()
    {
        var order = CreateConfirmedOrder();

        var result = order.Cancel();

        result.IsSuccess.Should().BeTrue();
        order.State.Should().Be(OrderState.Cancelled);
    }

    [Fact]
    public void Cancel_WhenPreparing_ShouldSucceed()
    {
        var order = CreatePreparingOrder();

        var result = order.Cancel();

        result.IsSuccess.Should().BeTrue();
        order.State.Should().Be(OrderState.Cancelled);
    }

    [Fact]
    public void Cancel_WhenReadyForDelivery_ShouldSucceed()
    {
        var order = CreateReadyForDeliveryOrder();

        var result = order.Cancel();

        result.IsSuccess.Should().BeTrue();
        order.State.Should().Be(OrderState.Cancelled);
    }

    [Fact]
    public void Cancel_WhenOutForDelivery_ShouldFail()
    {
        var order = CreateOutForDeliveryOrder();

        var result = order.Cancel();

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.InvalidTransition");
    }

    [Fact]
    public void Cancel_WhenDelivered_ShouldFail()
    {
        var order = CreateOutForDeliveryOrder();
        order.Deliver(_driverId);

        var result = order.Cancel();

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.InvalidTransition");
    }

    [Fact]
    public void Cancel_ShouldDispatchEvents()
    {
        var order = CreatePendingOrder();

        order.Cancel("system");

        order.DomainEvents.Should().Contain(e => e is OrderCancelledDomainEvent);
        order.DomainEvents.OfType<OrderCancelledDomainEvent>().Should()
            .Contain(e => e.CancelledBy == "system");
    }

    [Fact]
    public void CanBeCancelled_WhenPending_ShouldReturnTrue()
    {
        var order = CreatePendingOrder();

        order.CanBeCancelled().Should().BeTrue();
    }

    [Fact]
    public void CanBeCancelled_WhenOutForDelivery_ShouldReturnFalse()
    {
        var order = CreateOutForDeliveryOrder();

        order.CanBeCancelled().Should().BeFalse();
    }

    [Fact]
    public void IsTerminalState_WhenDelivered_ShouldReturnTrue()
    {
        var order = CreateOutForDeliveryOrder();
        order.Deliver(_driverId);

        order.IsTerminalState().Should().BeTrue();
    }

    [Fact]
    public void IsTerminalState_WhenCancelled_ShouldReturnTrue()
    {
        var order = CreatePendingOrder();
        order.Cancel();

        order.IsTerminalState().Should().BeTrue();
    }

    [Fact]
    public void IsTerminalState_WhenPending_ShouldReturnFalse()
    {
        var order = CreatePendingOrder();

        order.IsTerminalState().Should().BeFalse();
    }

    // --- OrderItem Tests ---

    [Fact]
    public void OrderItem_Create_WithValidData_ShouldSucceed()
    {
        var result = OrderItem.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Hamburger",
            Money.Create(25.00m).Value,
            2);

        result.IsSuccess.Should().BeTrue();
        result.Value.ProductName.Should().Be("Hamburger");
        result.Value.Quantity.Should().Be(2);
        result.Value.UnitPrice.Amount.Should().Be(25.00m);
    }

    [Fact]
    public void OrderItem_Create_WithEmptyOrderId_ShouldSucceed()
    {
        var result = OrderItem.Create(
            Guid.Empty,
            Guid.NewGuid(),
            "Hamburger",
            Money.Create(25.00m).Value,
            2);

        result.IsSuccess.Should().BeTrue();
        result.Value.OrderId.Should().Be(Guid.Empty);
    }

    [Fact]
    public void OrderItem_Create_WithEmptyProductId_ShouldFail()
    {
        var result = OrderItem.Create(
            Guid.NewGuid(),
            Guid.Empty,
            "Hamburger",
            Money.Create(25.00m).Value,
            2);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("OrderItem.ProductRequired");
    }

    [Fact]
    public void OrderItem_Create_WithEmptyProductName_ShouldFail()
    {
        var result = OrderItem.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "",
            Money.Create(25.00m).Value,
            2);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("OrderItem.ProductNameRequired");
    }

    [Fact]
    public void OrderItem_Create_WithProductNameTooLong_ShouldFail()
    {
        var longName = new string('A', 201);

        var result = OrderItem.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            longName,
            Money.Create(25.00m).Value,
            2);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("OrderItem.ProductNameTooLong");
    }

    [Fact]
    public void OrderItem_Create_WithNegativePrice_ShouldFail()
    {
        var result = OrderItem.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Hamburger",
            CreateNegativeMoney(-5.00m),
            2);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("OrderItem.InvalidPrice");
    }

    [Fact]
    public void OrderItem_Create_WithZeroQuantity_ShouldFail()
    {
        var result = OrderItem.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Hamburger",
            Money.Create(25.00m).Value,
            0);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("OrderItem.InvalidQuantity");
    }

    [Fact]
    public void OrderItem_Create_WithNegativeQuantity_ShouldFail()
    {
        var result = OrderItem.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Hamburger",
            Money.Create(25.00m).Value,
            -1);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("OrderItem.InvalidQuantity");
    }

    [Fact]
    public void OrderItem_Subtotal_ShouldReturnPriceTimesQuantity()
    {
        var item = OrderItem.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Hamburger",
            Money.Create(25.00m).Value,
            3).Value;

        item.Subtotal.Amount.Should().Be(75.00m);
    }

    // --- Helpers ---

    private OrderItem CreateOrderItem(decimal unitPrice, int quantity)
    {
        return OrderItem.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Product",
            Money.Create(unitPrice).Value,
            quantity).Value;
    }

    private Order CreatePendingOrder()
    {
        var items = new List<OrderItem> { CreateOrderItem(10.00m, 1) };
        return Order.Create(
            _tenantId,
            _customerId,
            items,
            Money.Create(5.00m).Value,
            ValidAddress).Value;
    }

    private Order CreateConfirmedOrder()
    {
        var order = CreatePendingOrder();
        order.Confirm();
        return order;
    }

    private Order CreatePreparingOrder()
    {
        var order = CreateConfirmedOrder();
        order.StartPreparing();
        return order;
    }

    private Order CreateReadyForDeliveryOrder()
    {
        var order = CreatePreparingOrder();
        order.MarkReadyForDelivery();
        return order;
    }

    private Order CreateOutForDeliveryOrder()
    {
        var order = CreateReadyForDeliveryOrder();
        order.StartDelivery(_driverId);
        return order;
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
