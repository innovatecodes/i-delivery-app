using FluentAssertions;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.Application.Commands.Payments;
using IDelivery.Application.Events.Handlers;
using IDelivery.Domain.Common.ValueObjects;
using IDelivery.Domain.Payments.Entities;
using IDelivery.Domain.Payments.Enums;
using Moq;
using Xunit;

namespace IDelivery.UnitTests.Application.Payments;

public class CreatePaymentCommandHandlerTests
{
    private readonly Mock<IPaymentRepository> _mockPaymentRepository;
    private readonly CreatePaymentCommandHandler _handler;

    public CreatePaymentCommandHandlerTests()
    {
        _mockPaymentRepository = new Mock<IPaymentRepository>();
        _handler = new CreatePaymentCommandHandler(_mockPaymentRepository.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreatePayment()
    {
        var command = new CreatePaymentCommand(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            25.50m, "BRL", PaymentMethod.Cash);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);
        _mockPaymentRepository.Verify(x => x.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithExistingPaymentForOrder_ShouldFail()
    {
        var orderId = Guid.NewGuid();
        var existingPayment = Payment.Create(orderId, Guid.NewGuid(), Guid.NewGuid(),
            Money.Create(25.50m, "BRL").Value, PaymentMethod.Cash).Value;

        _mockPaymentRepository.Setup(x => x.GetByOrderIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPayment);

        var command = new CreatePaymentCommand(
            orderId, Guid.NewGuid(), Guid.NewGuid(),
            25.50m, "BRL", PaymentMethod.Cash);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Payment.AlreadyExists");
    }

    [Fact]
    public async Task Handle_WithInvalidAmount_ShouldFail()
    {
        var command = new CreatePaymentCommand(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            0, "BRL", PaymentMethod.Cash);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }
}

public class MarkPaymentAsPaidCommandHandlerTests
{
    private readonly Mock<IPaymentRepository> _mockPaymentRepository;
    private readonly MarkPaymentAsPaidCommandHandler _handler;

    public MarkPaymentAsPaidCommandHandlerTests()
    {
        _mockPaymentRepository = new Mock<IPaymentRepository>();
        _handler = new MarkPaymentAsPaidCommandHandler(_mockPaymentRepository.Object);
    }

    [Fact]
    public async Task Handle_WithPendingPayment_ShouldMarkAsPaid()
    {
        var payment = Payment.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            Money.Create(25.50m, "BRL").Value, PaymentMethod.Cash).Value;

        _mockPaymentRepository.Setup(x => x.GetByIdAsync(payment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        var command = new MarkPaymentAsPaidCommand(payment.Id);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        payment.Status.Should().Be(PaymentStatus.Paid);
        _mockPaymentRepository.Verify(x => x.Update(payment), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentPayment_ShouldFail()
    {
        _mockPaymentRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);

        var command = new MarkPaymentAsPaidCommand(Guid.NewGuid());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Payment.NotFound");
    }

    [Fact]
    public async Task Handle_WithAlreadyPaidPayment_ShouldFail()
    {
        var payment = Payment.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            Money.Create(25.50m, "BRL").Value, PaymentMethod.Cash).Value;
        payment.MarkAsPaid();

        _mockPaymentRepository.Setup(x => x.GetByIdAsync(payment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        var command = new MarkPaymentAsPaidCommand(payment.Id);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Payment.InvalidStatus");
    }
}

public class OrderDeliveredPaymentHandlerTests
{
    private readonly Mock<IPaymentRepository> _mockPaymentRepository;
    private readonly OrderDeliveredPaymentHandler _handler;

    public OrderDeliveredPaymentHandlerTests()
    {
        _mockPaymentRepository = new Mock<IPaymentRepository>();
        _handler = new OrderDeliveredPaymentHandler(
            _mockPaymentRepository.Object,
            new Microsoft.Extensions.Logging.Abstractions.NullLogger<OrderDeliveredPaymentHandler>());
    }

    [Fact]
    public async Task Handle_WithPendingPayment_ShouldMarkAsPaid()
    {
        var orderId = Guid.NewGuid();
        var payment = Payment.Create(orderId, Guid.NewGuid(), Guid.NewGuid(),
            Money.Create(25.50m, "BRL").Value, PaymentMethod.Cash).Value;

        _mockPaymentRepository.Setup(x => x.GetByOrderIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        var domainEvent = new IDelivery.Domain.Orders.Events.OrderDeliveredDomainEvent(
            orderId, payment.TenantId, payment.CustomerId, Guid.NewGuid(), payment.Amount);

        await _handler.Handle(domainEvent, CancellationToken.None);

        payment.Status.Should().Be(PaymentStatus.Paid);
        _mockPaymentRepository.Verify(x => x.Update(payment), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNoPayment_ShouldNotThrow()
    {
        var orderId = Guid.NewGuid();

        _mockPaymentRepository.Setup(x => x.GetByOrderIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);

        var domainEvent = new IDelivery.Domain.Orders.Events.OrderDeliveredDomainEvent(
            orderId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Money.Create(25.50m, "BRL").Value);

        var act = async () => await _handler.Handle(domainEvent, CancellationToken.None);

        await act.Should().NotThrowAsync();
    }
}
