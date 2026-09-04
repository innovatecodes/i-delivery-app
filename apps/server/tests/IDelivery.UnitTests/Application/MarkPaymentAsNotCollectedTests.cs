using FluentAssertions;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.Application.Commands.Payments;
using IDelivery.Domain.Common.ValueObjects;
using IDelivery.Domain.Payments.Entities;
using IDelivery.Domain.Payments.Enums;
using Moq;
using Xunit;

namespace IDelivery.UnitTests.Application.Payments;

public class MarkPaymentAsNotCollectedCommandHandlerTests
{
    private readonly Mock<IPaymentRepository> _mockPaymentRepository;
    private readonly MarkPaymentAsNotCollectedCommandHandler _handler;

    public MarkPaymentAsNotCollectedCommandHandlerTests()
    {
        _mockPaymentRepository = new Mock<IPaymentRepository>();
        _handler = new MarkPaymentAsNotCollectedCommandHandler(_mockPaymentRepository.Object);
    }

    [Fact]
    public async Task Handle_WithPendingPayment_ShouldMarkAsNotCollected()
    {
        var payment = Payment.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            Money.Create(25.50m, "BRL").Value, PaymentMethod.Cash).Value;

        _mockPaymentRepository.Setup(x => x.GetByIdAsync(payment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        var command = new MarkPaymentAsNotCollectedCommand(payment.Id);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        payment.Status.Should().Be(PaymentStatus.NotCollected);
        _mockPaymentRepository.Verify(x => x.Update(payment), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentPayment_ShouldFail()
    {
        _mockPaymentRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);

        var command = new MarkPaymentAsNotCollectedCommand(Guid.NewGuid());

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

        var command = new MarkPaymentAsNotCollectedCommand(payment.Id);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Payment.InvalidStatus");
    }
}
