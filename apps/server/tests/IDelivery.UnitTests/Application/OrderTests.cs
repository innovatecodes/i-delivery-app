using IDelivery.Application.Commands.Orders;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.Application.Abstractions.Authentication;
using IDelivery.Domain.Carts.Entities;
using IDelivery.Domain.Common.ValueObjects;
using IDelivery.Domain.Customers.Entities;
using IDelivery.Domain.Delivery.Entities;
using IDelivery.Domain.Delivery.Enums;
using IDelivery.Domain.Orders.Entities;
using IDelivery.Domain.Orders.Enums;
using IDelivery.SharedKernel.Common.Result;
using FluentAssertions;
using Moq;
using System;
using System.Threading;
using Xunit;

namespace IDelivery.UnitTests.Application;

public class ConfirmOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _mockOrderRepository;
    private readonly Mock<ITenantContext> _mockTenantContext;
    private readonly Guid _tenantId = Guid.NewGuid();

    public ConfirmOrderCommandHandlerTests()
    {
        _mockOrderRepository = new Mock<IOrderRepository>();
        _mockTenantContext = new Mock<ITenantContext>();
        _mockTenantContext.Setup(x => x.TenantId).Returns(_tenantId);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldConfirmOrder()
    {
        var orderId = Guid.NewGuid();
        var order = OrderTestHelper.CreateOrder(_tenantId, OrderState.Pending);

        _mockOrderRepository.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var command = new ConfirmOrderCommand(orderId);

        var handler = new ConfirmOrderCommandHandler(
            _mockOrderRepository.Object,
            _mockTenantContext.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        order.State.Should().Be(OrderState.Confirmed);
        _mockOrderRepository.Verify(x => x.UpdateAsync(order, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithoutTenant_ShouldFail()
    {
        _mockTenantContext.Setup(x => x.TenantId).Returns((Guid?)null);

        var command = new ConfirmOrderCommand(Guid.NewGuid());

        var handler = new ConfirmOrderCommandHandler(
            _mockOrderRepository.Object,
            _mockTenantContext.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.TenantRequired");
    }

    [Fact]
    public async Task Handle_WithNonExistentOrder_ShouldFail()
    {
        _mockOrderRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var command = new ConfirmOrderCommand(Guid.NewGuid());

        var handler = new ConfirmOrderCommandHandler(
            _mockOrderRepository.Object,
            _mockTenantContext.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.NotFound");
    }

    [Fact]
    public async Task Handle_WithTenantMismatch_ShouldFail()
    {
        var orderId = Guid.NewGuid();
        var order = OrderTestHelper.CreateOrder(Guid.NewGuid(), OrderState.Pending);

        _mockOrderRepository.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var command = new ConfirmOrderCommand(orderId);

        var handler = new ConfirmOrderCommandHandler(
            _mockOrderRepository.Object,
            _mockTenantContext.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.AccessDenied");
    }

    [Fact]
    public async Task Handle_WithInvalidStateTransition_ShouldFail()
    {
        var orderId = Guid.NewGuid();
        var order = OrderTestHelper.CreateOrder(_tenantId, OrderState.Confirmed);

        _mockOrderRepository.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var command = new ConfirmOrderCommand(orderId);

        var handler = new ConfirmOrderCommandHandler(
            _mockOrderRepository.Object,
            _mockTenantContext.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.InvalidTransition");
    }
}

public class CancelOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _mockOrderRepository;
    private readonly Mock<ITenantContext> _mockTenantContext;
    private readonly Mock<ICurrentUser> _mockCurrentUser;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    public CancelOrderCommandHandlerTests()
    {
        _mockOrderRepository = new Mock<IOrderRepository>();
        _mockTenantContext = new Mock<ITenantContext>();
        _mockCurrentUser = new Mock<ICurrentUser>();
        _mockTenantContext.Setup(x => x.TenantId).Returns(_tenantId);
        _mockCurrentUser.Setup(x => x.UserId).Returns(_userId);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCancelOrder()
    {
        var orderId = Guid.NewGuid();
        var order = OrderTestHelper.CreateOrder(_tenantId, OrderState.Pending);

        _mockOrderRepository.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var command = new CancelOrderCommand(orderId, "Customer");

        var handler = new CancelOrderCommandHandler(
            _mockOrderRepository.Object,
            _mockTenantContext.Object,
            _mockCurrentUser.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        order.State.Should().Be(OrderState.Cancelled);
        _mockOrderRepository.Verify(x => x.UpdateAsync(order, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithoutTenant_ShouldFail()
    {
        _mockTenantContext.Setup(x => x.TenantId).Returns((Guid?)null);

        var command = new CancelOrderCommand(Guid.NewGuid(), null);

        var handler = new CancelOrderCommandHandler(
            _mockOrderRepository.Object,
            _mockTenantContext.Object,
            _mockCurrentUser.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.TenantRequired");
    }

    [Fact]
    public async Task Handle_WithNonExistentOrder_ShouldFail()
    {
        _mockOrderRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var command = new CancelOrderCommand(Guid.NewGuid(), null);

        var handler = new CancelOrderCommandHandler(
            _mockOrderRepository.Object,
            _mockTenantContext.Object,
            _mockCurrentUser.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.NotFound");
    }

    [Fact]
    public async Task Handle_WithTenantMismatch_ShouldFail()
    {
        var orderId = Guid.NewGuid();
        var order = OrderTestHelper.CreateOrder(Guid.NewGuid(), OrderState.Pending);

        _mockOrderRepository.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var command = new CancelOrderCommand(orderId, null);

        var handler = new CancelOrderCommandHandler(
            _mockOrderRepository.Object,
            _mockTenantContext.Object,
            _mockCurrentUser.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.AccessDenied");
    }

    [Fact]
    public async Task Handle_WithInvalidStateTransition_ShouldFail()
    {
        var orderId = Guid.NewGuid();
        var order = OrderTestHelper.CreateOrder(_tenantId, OrderState.OutForDelivery);

        _mockOrderRepository.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var command = new CancelOrderCommand(orderId, null);

        var handler = new CancelOrderCommandHandler(
            _mockOrderRepository.Object,
            _mockTenantContext.Object,
            _mockCurrentUser.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.InvalidTransition");
    }
}

public class StartPreparingOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _mockOrderRepository;
    private readonly Mock<ITenantContext> _mockTenantContext;
    private readonly Guid _tenantId = Guid.NewGuid();

    public StartPreparingOrderCommandHandlerTests()
    {
        _mockOrderRepository = new Mock<IOrderRepository>();
        _mockTenantContext = new Mock<ITenantContext>();
        _mockTenantContext.Setup(x => x.TenantId).Returns(_tenantId);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldStartPreparing()
    {
        var orderId = Guid.NewGuid();
        var order = OrderTestHelper.CreateOrder(_tenantId, OrderState.Confirmed);

        _mockOrderRepository.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var command = new StartPreparingOrderCommand(orderId);

        var handler = new StartPreparingOrderCommandHandler(
            _mockOrderRepository.Object,
            _mockTenantContext.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        order.State.Should().Be(OrderState.Preparing);
        _mockOrderRepository.Verify(x => x.UpdateAsync(order, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithoutTenant_ShouldFail()
    {
        _mockTenantContext.Setup(x => x.TenantId).Returns((Guid?)null);

        var command = new StartPreparingOrderCommand(Guid.NewGuid());

        var handler = new StartPreparingOrderCommandHandler(
            _mockOrderRepository.Object,
            _mockTenantContext.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.TenantRequired");
    }

    [Fact]
    public async Task Handle_WithNonExistentOrder_ShouldFail()
    {
        _mockOrderRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var command = new StartPreparingOrderCommand(Guid.NewGuid());

        var handler = new StartPreparingOrderCommandHandler(
            _mockOrderRepository.Object,
            _mockTenantContext.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.NotFound");
    }

    [Fact]
    public async Task Handle_WithInvalidStateTransition_ShouldFail()
    {
        var orderId = Guid.NewGuid();
        var order = OrderTestHelper.CreateOrder(_tenantId, OrderState.Pending);

        _mockOrderRepository.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var command = new StartPreparingOrderCommand(orderId);

        var handler = new StartPreparingOrderCommandHandler(
            _mockOrderRepository.Object,
            _mockTenantContext.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.InvalidTransition");
    }
}

public class MarkOrderReadyForDeliveryCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _mockOrderRepository;
    private readonly Mock<ITenantContext> _mockTenantContext;
    private readonly Guid _tenantId = Guid.NewGuid();

    public MarkOrderReadyForDeliveryCommandHandlerTests()
    {
        _mockOrderRepository = new Mock<IOrderRepository>();
        _mockTenantContext = new Mock<ITenantContext>();
        _mockTenantContext.Setup(x => x.TenantId).Returns(_tenantId);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldMarkReadyForDelivery()
    {
        var orderId = Guid.NewGuid();
        var order = OrderTestHelper.CreateOrder(_tenantId, OrderState.Preparing);

        _mockOrderRepository.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var command = new MarkOrderReadyForDeliveryCommand(orderId);

        var handler = new MarkOrderReadyForDeliveryCommandHandler(
            _mockOrderRepository.Object,
            _mockTenantContext.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        order.State.Should().Be(OrderState.ReadyForDelivery);
        _mockOrderRepository.Verify(x => x.UpdateAsync(order, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithoutTenant_ShouldFail()
    {
        _mockTenantContext.Setup(x => x.TenantId).Returns((Guid?)null);

        var command = new MarkOrderReadyForDeliveryCommand(Guid.NewGuid());

        var handler = new MarkOrderReadyForDeliveryCommandHandler(
            _mockOrderRepository.Object,
            _mockTenantContext.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.TenantRequired");
    }

    [Fact]
    public async Task Handle_WithNonExistentOrder_ShouldFail()
    {
        _mockOrderRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var command = new MarkOrderReadyForDeliveryCommand(Guid.NewGuid());

        var handler = new MarkOrderReadyForDeliveryCommandHandler(
            _mockOrderRepository.Object,
            _mockTenantContext.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.NotFound");
    }

    [Fact]
    public async Task Handle_WithInvalidStateTransition_ShouldFail()
    {
        var orderId = Guid.NewGuid();
        var order = OrderTestHelper.CreateOrder(_tenantId, OrderState.Confirmed);

        _mockOrderRepository.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var command = new MarkOrderReadyForDeliveryCommand(orderId);

        var handler = new MarkOrderReadyForDeliveryCommandHandler(
            _mockOrderRepository.Object,
            _mockTenantContext.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.InvalidTransition");
    }
}

public class StartDeliveryCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _mockOrderRepository;
    private readonly Mock<ITenantContext> _mockTenantContext;
    private readonly Guid _tenantId = Guid.NewGuid();

    public StartDeliveryCommandHandlerTests()
    {
        _mockOrderRepository = new Mock<IOrderRepository>();
        _mockTenantContext = new Mock<ITenantContext>();
        _mockTenantContext.Setup(x => x.TenantId).Returns(_tenantId);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldStartDelivery()
    {
        var orderId = Guid.NewGuid();
        var driverId = Guid.NewGuid();
        var order = OrderTestHelper.CreateOrder(_tenantId, OrderState.ReadyForDelivery);

        _mockOrderRepository.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var command = new StartDeliveryCommand(orderId, driverId);

        var handler = new StartDeliveryCommandHandler(
            _mockOrderRepository.Object,
            _mockTenantContext.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        order.State.Should().Be(OrderState.OutForDelivery);
        order.DeliveryDriverId.Should().Be(driverId);
        _mockOrderRepository.Verify(x => x.UpdateAsync(order, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithoutTenant_ShouldFail()
    {
        _mockTenantContext.Setup(x => x.TenantId).Returns((Guid?)null);

        var command = new StartDeliveryCommand(Guid.NewGuid(), Guid.NewGuid());

        var handler = new StartDeliveryCommandHandler(
            _mockOrderRepository.Object,
            _mockTenantContext.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.TenantRequired");
    }

    [Fact]
    public async Task Handle_WithNonExistentOrder_ShouldFail()
    {
        _mockOrderRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var command = new StartDeliveryCommand(Guid.NewGuid(), Guid.NewGuid());

        var handler = new StartDeliveryCommandHandler(
            _mockOrderRepository.Object,
            _mockTenantContext.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.NotFound");
    }

    [Fact]
    public async Task Handle_WithInvalidStateTransition_ShouldFail()
    {
        var orderId = Guid.NewGuid();
        var order = OrderTestHelper.CreateOrder(_tenantId, OrderState.Preparing);

        _mockOrderRepository.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var command = new StartDeliveryCommand(orderId, Guid.NewGuid());

        var handler = new StartDeliveryCommandHandler(
            _mockOrderRepository.Object,
            _mockTenantContext.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.InvalidTransition");
    }
}

public class DeliverOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _mockOrderRepository;
    private readonly Mock<ITenantContext> _mockTenantContext;
    private readonly Mock<ICurrentUser> _mockCurrentUser;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    public DeliverOrderCommandHandlerTests()
    {
        _mockOrderRepository = new Mock<IOrderRepository>();
        _mockTenantContext = new Mock<ITenantContext>();
        _mockCurrentUser = new Mock<ICurrentUser>();
        _mockTenantContext.Setup(x => x.TenantId).Returns(_tenantId);
        _mockCurrentUser.Setup(x => x.UserId).Returns(_userId);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldDeliverOrder()
    {
        var orderId = Guid.NewGuid();
        var order = OrderTestHelper.CreateOrder(_tenantId, OrderState.OutForDelivery, _userId);

        _mockOrderRepository.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var command = new DeliverOrderCommand(orderId);

        var handler = new DeliverOrderCommandHandler(
            _mockOrderRepository.Object,
            _mockTenantContext.Object,
            _mockCurrentUser.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        order.State.Should().Be(OrderState.Delivered);
        _mockOrderRepository.Verify(x => x.UpdateAsync(order, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithoutTenant_ShouldFail()
    {
        _mockTenantContext.Setup(x => x.TenantId).Returns((Guid?)null);

        var command = new DeliverOrderCommand(Guid.NewGuid());

        var handler = new DeliverOrderCommandHandler(
            _mockOrderRepository.Object,
            _mockTenantContext.Object,
            _mockCurrentUser.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.TenantRequired");
    }

    [Fact]
    public async Task Handle_WithoutUser_ShouldFail()
    {
        _mockCurrentUser.Setup(x => x.UserId).Returns((Guid?)null);

        var command = new DeliverOrderCommand(Guid.NewGuid());

        var handler = new DeliverOrderCommandHandler(
            _mockOrderRepository.Object,
            _mockTenantContext.Object,
            _mockCurrentUser.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.UserRequired");
    }

    [Fact]
    public async Task Handle_WithNonExistentOrder_ShouldFail()
    {
        _mockOrderRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var command = new DeliverOrderCommand(Guid.NewGuid());

        var handler = new DeliverOrderCommandHandler(
            _mockOrderRepository.Object,
            _mockTenantContext.Object,
            _mockCurrentUser.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.NotFound");
    }

    [Fact]
    public async Task Handle_WithUnauthorizedDriver_ShouldFail()
    {
        var orderId = Guid.NewGuid();
        var order = OrderTestHelper.CreateOrder(_tenantId, OrderState.OutForDelivery);
        order.StartDelivery(Guid.NewGuid());

        _mockOrderRepository.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var command = new DeliverOrderCommand(orderId);

        var handler = new DeliverOrderCommandHandler(
            _mockOrderRepository.Object,
            _mockTenantContext.Object,
            _mockCurrentUser.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.UnauthorizedDriver");
    }
}

public class FailDeliveryCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _mockOrderRepository;
    private readonly Mock<ITenantContext> _mockTenantContext;
    private readonly Mock<ICurrentUser> _mockCurrentUser;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    public FailDeliveryCommandHandlerTests()
    {
        _mockOrderRepository = new Mock<IOrderRepository>();
        _mockTenantContext = new Mock<ITenantContext>();
        _mockCurrentUser = new Mock<ICurrentUser>();
        _mockTenantContext.Setup(x => x.TenantId).Returns(_tenantId);
        _mockCurrentUser.Setup(x => x.UserId).Returns(_userId);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldFailDelivery()
    {
        var orderId = Guid.NewGuid();
        var order = OrderTestHelper.CreateOrder(_tenantId, OrderState.OutForDelivery, _userId);

        _mockOrderRepository.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var command = new FailDeliveryCommand(orderId, DeliveryFailureReason.CustomerAbsent, null);

        var handler = new FailDeliveryCommandHandler(
            _mockOrderRepository.Object,
            _mockTenantContext.Object,
            _mockCurrentUser.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        order.State.Should().Be(OrderState.DeliveryFailed);
        _mockOrderRepository.Verify(x => x.UpdateAsync(order, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithoutTenant_ShouldFail()
    {
        _mockTenantContext.Setup(x => x.TenantId).Returns((Guid?)null);

        var command = new FailDeliveryCommand(Guid.NewGuid(), DeliveryFailureReason.Other, "reason");

        var handler = new FailDeliveryCommandHandler(
            _mockOrderRepository.Object,
            _mockTenantContext.Object,
            _mockCurrentUser.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.TenantRequired");
    }

    [Fact]
    public async Task Handle_WithoutUser_ShouldFail()
    {
        _mockCurrentUser.Setup(x => x.UserId).Returns((Guid?)null);

        var command = new FailDeliveryCommand(Guid.NewGuid(), DeliveryFailureReason.Other, "reason");

        var handler = new FailDeliveryCommandHandler(
            _mockOrderRepository.Object,
            _mockTenantContext.Object,
            _mockCurrentUser.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.UserRequired");
    }

    [Fact]
    public async Task Handle_WithNonExistentOrder_ShouldFail()
    {
        _mockOrderRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var command = new FailDeliveryCommand(Guid.NewGuid(), DeliveryFailureReason.Other, "reason");

        var handler = new FailDeliveryCommandHandler(
            _mockOrderRepository.Object,
            _mockTenantContext.Object,
            _mockCurrentUser.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.NotFound");
    }

    [Fact]
    public async Task Handle_WithUnauthorizedDriver_ShouldFail()
    {
        var orderId = Guid.NewGuid();
        var order = OrderTestHelper.CreateOrder(_tenantId, OrderState.OutForDelivery);
        order.StartDelivery(Guid.NewGuid());

        _mockOrderRepository.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var command = new FailDeliveryCommand(orderId, DeliveryFailureReason.CustomerAbsent, null);

        var handler = new FailDeliveryCommandHandler(
            _mockOrderRepository.Object,
            _mockTenantContext.Object,
            _mockCurrentUser.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.UnauthorizedDriver");
    }

    [Fact]
    public async Task Handle_WithOtherReasonAndNoDetail_ShouldFail()
    {
        var orderId = Guid.NewGuid();
        var order = OrderTestHelper.CreateOrder(_tenantId, OrderState.OutForDelivery, _userId);

        _mockOrderRepository.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var command = new FailDeliveryCommand(orderId, DeliveryFailureReason.Other, null);

        var handler = new FailDeliveryCommandHandler(
            _mockOrderRepository.Object,
            _mockTenantContext.Object,
            _mockCurrentUser.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.FailureReasonDetailRequired");
    }
}

public class CreateOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _mockOrderRepository;
    private readonly Mock<ICartRepository> _mockCartRepository;
    private readonly Mock<ICustomerRepository> _mockCustomerRepository;
    private readonly Mock<IDeliverySettingsRepository> _mockDeliverySettingsRepository;
    private readonly Mock<ITenantContext> _mockTenantContext;
    private readonly Mock<ICurrentUser> _mockCurrentUser;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    public CreateOrderCommandHandlerTests()
    {
        _mockOrderRepository = new Mock<IOrderRepository>();
        _mockCartRepository = new Mock<ICartRepository>();
        _mockCustomerRepository = new Mock<ICustomerRepository>();
        _mockDeliverySettingsRepository = new Mock<IDeliverySettingsRepository>();
        _mockTenantContext = new Mock<ITenantContext>();
        _mockCurrentUser = new Mock<ICurrentUser>();
        _mockTenantContext.Setup(x => x.TenantId).Returns(_tenantId);
        _mockCurrentUser.Setup(x => x.UserId).Returns(_userId);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateOrder()
    {
        var productId = Guid.NewGuid();

        var customerResult = Customer.Create(_tenantId, _userId, "John Doe", Email.Create("john@test.com").Value);
        Assert.True(customerResult.IsSuccess);
        var customer = customerResult.Value;

        var cart = OrderTestHelper.CreateCartWithItems(_tenantId, _userId, productId, "Coca-Cola", 5.50m, 2);

        _mockCustomerRepository.Setup(x => x.GetByUserIdAsync(_tenantId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);
        _mockCartRepository.Setup(x => x.GetByUserIdAsync(_tenantId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);
        _mockDeliverySettingsRepository.Setup(x => x.GetByTenantIdAsync(_tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DeliverySettings?)null);

        var command = new CreateOrderCommand(
            new List<CreateOrderItemDto>
            {
                new(productId, "Coca-Cola", 5.50m, "BRL", 2)
            },
            10.00m,
            "BRL",
            "Rua A",
            "123",
            null,
            "Centro",
            "São Paulo",
            "SP",
            "01001-000",
            null,
            5.0m);

        var handler = new CreateOrderCommandHandler(
            _mockOrderRepository.Object,
            _mockCartRepository.Object,
            _mockCustomerRepository.Object,
            _mockDeliverySettingsRepository.Object,
            _mockTenantContext.Object,
            _mockCurrentUser.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        _mockOrderRepository.Verify(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockCartRepository.Verify(x => x.UpdateAsync(It.IsAny<Cart>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithoutTenant_ShouldFail()
    {
        _mockTenantContext.Setup(x => x.TenantId).Returns((Guid?)null);

        var command = new CreateOrderCommand(
            new List<CreateOrderItemDto>(),
            0,
            "BRL",
            "Rua A",
            "123",
            null,
            "Centro",
            "São Paulo",
            "SP",
            "01001-000",
            null,
            null);

        var handler = new CreateOrderCommandHandler(
            _mockOrderRepository.Object,
            _mockCartRepository.Object,
            _mockCustomerRepository.Object,
            _mockDeliverySettingsRepository.Object,
            _mockTenantContext.Object,
            _mockCurrentUser.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.TenantRequired");
    }

    [Fact]
    public async Task Handle_WithoutUser_ShouldFail()
    {
        _mockCurrentUser.Setup(x => x.UserId).Returns((Guid?)null);

        var command = new CreateOrderCommand(
            new List<CreateOrderItemDto>(),
            0,
            "BRL",
            "Rua A",
            "123",
            null,
            "Centro",
            "São Paulo",
            "SP",
            "01001-000",
            null,
            null);

        var handler = new CreateOrderCommandHandler(
            _mockOrderRepository.Object,
            _mockCartRepository.Object,
            _mockCustomerRepository.Object,
            _mockDeliverySettingsRepository.Object,
            _mockTenantContext.Object,
            _mockCurrentUser.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.UserRequired");
    }

    [Fact]
    public async Task Handle_WithNonExistentCustomer_ShouldFail()
    {
        _mockCustomerRepository.Setup(x => x.GetByUserIdAsync(_tenantId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        var command = new CreateOrderCommand(
            new List<CreateOrderItemDto>(),
            0,
            "BRL",
            "Rua A",
            "123",
            null,
            "Centro",
            "São Paulo",
            "SP",
            "01001-000",
            null,
            null);

        var handler = new CreateOrderCommandHandler(
            _mockOrderRepository.Object,
            _mockCartRepository.Object,
            _mockCustomerRepository.Object,
            _mockDeliverySettingsRepository.Object,
            _mockTenantContext.Object,
            _mockCurrentUser.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.CustomerNotFound");
    }

    [Fact]
    public async Task Handle_WithEmptyCart_ShouldFail()
    {
        var customerResult = Customer.Create(_tenantId, _userId, "John Doe", Email.Create("john@test.com").Value);
        Assert.True(customerResult.IsSuccess);

        var cart = Cart.Create(_tenantId, _userId).Value;

        _mockCustomerRepository.Setup(x => x.GetByUserIdAsync(_tenantId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customerResult.Value);
        _mockCartRepository.Setup(x => x.GetByUserIdAsync(_tenantId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        var command = new CreateOrderCommand(
            new List<CreateOrderItemDto>(),
            0,
            "BRL",
            "Rua A",
            "123",
            null,
            "Centro",
            "São Paulo",
            "SP",
            "01001-000",
            null,
            null);

        var handler = new CreateOrderCommandHandler(
            _mockOrderRepository.Object,
            _mockCartRepository.Object,
            _mockCustomerRepository.Object,
            _mockDeliverySettingsRepository.Object,
            _mockTenantContext.Object,
            _mockCurrentUser.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.CartEmpty");
    }
}

internal static class OrderTestHelper
{
    internal static Order CreateOrder(Guid tenantId, OrderState state, Guid? driverId = null)
    {
        var customerId = Guid.NewGuid();
        var money = Money.Create(10.00m, "BRL").Value;
        var address = Address.Create("Rua A", "123", null, "Centro", "São Paulo", "SP", "01001-000").Value;
        var item = OrderItem.Create(Guid.NewGuid(), Guid.NewGuid(), "Product", money, 1).Value;

        var orderResult = Order.Create(
            tenantId,
            customerId,
            new List<OrderItem> { item },
            Money.Zero("BRL"),
            address,
            5.0m);

        Assert.True(orderResult.IsSuccess);
        var order = orderResult.Value;

        for (var i = 0; i < (int)state; i++)
        {
            var transitionResult = GetNextTransition(order, driverId);
            Assert.True(transitionResult.IsSuccess);
        }

        return order;
    }

    internal static Cart CreateCartWithItems(Guid tenantId, Guid userId, Guid productId, string productName, decimal unitPrice, int quantity)
    {
        var cart = Cart.Create(tenantId, userId).Value;
        var money = Money.Create(unitPrice, "BRL").Value;
        cart.AddItem(productId, productName, money, quantity);
        return cart;
    }

    private static Result GetNextTransition(Order order, Guid? driverId = null)
    {
        return order.State switch
        {
            OrderState.Pending => order.Confirm(),
            OrderState.Confirmed => order.StartPreparing(),
            OrderState.Preparing => order.MarkReadyForDelivery(),
            OrderState.ReadyForDelivery => order.StartDelivery(driverId ?? Guid.NewGuid()),
            OrderState.OutForDelivery => order.Deliver(order.DeliveryDriverId!.Value),
            _ => Result.Failure(new Error("Order.NoTransition", "No transition available"))
        };
    }
}
