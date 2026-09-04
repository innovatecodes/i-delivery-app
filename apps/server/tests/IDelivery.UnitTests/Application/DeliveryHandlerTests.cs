using FluentAssertions;
using IDelivery.Application.Abstractions.Authentication;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.Application.Commands.Delivery;
using IDelivery.Domain.Delivery.Entities;
using IDelivery.Domain.Delivery.Enums;
using Moq;
using Xunit;

namespace IDelivery.UnitTests.Application;

public class CreateDeliverySettingsCommandHandlerTests
{
    private readonly Mock<IDeliverySettingsRepository> _mockDeliverySettingsRepository;
    private readonly Mock<ITenantContext> _mockTenantContext;
    private readonly Guid _tenantId = Guid.NewGuid();

    public CreateDeliverySettingsCommandHandlerTests()
    {
        _mockDeliverySettingsRepository = new Mock<IDeliverySettingsRepository>();
        _mockTenantContext = new Mock<ITenantContext>();
        _mockTenantContext.Setup(x => x.TenantId).Returns(_tenantId);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateDeliverySettings()
    {
        var command = new CreateDeliverySettingsCommand(
            DeliveryFeeType.Fixed, 5.00m, null, null, null, null);

        var handler = new CreateDeliverySettingsCommandHandler(
            _mockDeliverySettingsRepository.Object,
            _mockTenantContext.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        _mockDeliverySettingsRepository.Verify(x => x.AddAsync(It.IsAny<DeliverySettings>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithoutTenant_ShouldFail()
    {
        _mockTenantContext.Setup(x => x.TenantId).Returns((Guid?)null);

        var command = new CreateDeliverySettingsCommand(
            DeliveryFeeType.Fixed, 5.00m, null, null, null, null);

        var handler = new CreateDeliverySettingsCommandHandler(
            _mockDeliverySettingsRepository.Object,
            _mockTenantContext.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("DeliverySettings.TenantRequired");
    }

    [Fact]
    public async Task Handle_WhenSettingsAlreadyExist_ShouldFail()
    {
        var existing = DeliverySettings.Create(
            _tenantId, DeliveryFeeType.Fixed, IDelivery.Domain.Common.ValueObjects.Money.Create(5.00m).Value).Value;
        _mockDeliverySettingsRepository.Setup(x => x.GetByTenantIdAsync(_tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var command = new CreateDeliverySettingsCommand(
            DeliveryFeeType.Fixed, 5.00m, null, null, null, null);

        var handler = new CreateDeliverySettingsCommandHandler(
            _mockDeliverySettingsRepository.Object,
            _mockTenantContext.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("DeliverySettings.AlreadyExists");
    }
}

public class UpdateDeliverySettingsCommandHandlerTests
{
    private readonly Mock<IDeliverySettingsRepository> _mockDeliverySettingsRepository;
    private readonly Mock<ITenantContext> _mockTenantContext;
    private readonly Guid _tenantId = Guid.NewGuid();

    public UpdateDeliverySettingsCommandHandlerTests()
    {
        _mockDeliverySettingsRepository = new Mock<IDeliverySettingsRepository>();
        _mockTenantContext = new Mock<ITenantContext>();
        _mockTenantContext.Setup(x => x.TenantId).Returns(_tenantId);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateDeliverySettings()
    {
        var settings = DeliverySettings.Create(
            _tenantId, DeliveryFeeType.Fixed, IDelivery.Domain.Common.ValueObjects.Money.Create(5.00m).Value).Value;
        _mockDeliverySettingsRepository.Setup(x => x.GetByTenantIdAsync(_tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(settings);

        var command = new UpdateDeliverySettingsCommand(
            DeliveryFeeType.PerDistance, 8.00m, null, 2.50m, 3.00m, 20.00m);

        var handler = new UpdateDeliverySettingsCommandHandler(
            _mockDeliverySettingsRepository.Object,
            _mockTenantContext.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _mockDeliverySettingsRepository.Verify(x => x.UpdateAsync(settings, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithoutTenant_ShouldFail()
    {
        _mockTenantContext.Setup(x => x.TenantId).Returns((Guid?)null);

        var command = new UpdateDeliverySettingsCommand(
            DeliveryFeeType.Fixed, 5.00m, null, null, null, null);

        var handler = new UpdateDeliverySettingsCommandHandler(
            _mockDeliverySettingsRepository.Object,
            _mockTenantContext.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("DeliverySettings.TenantRequired");
    }

    [Fact]
    public async Task Handle_WhenSettingsNotFound_ShouldFail()
    {
        _mockDeliverySettingsRepository.Setup(x => x.GetByTenantIdAsync(_tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DeliverySettings?)null);

        var command = new UpdateDeliverySettingsCommand(
            DeliveryFeeType.Fixed, 5.00m, null, null, null, null);

        var handler = new UpdateDeliverySettingsCommandHandler(
            _mockDeliverySettingsRepository.Object,
            _mockTenantContext.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("DeliverySettings.NotFound");
    }
}

public class DeleteDeliverySettingsCommandHandlerTests
{
    private readonly Mock<IDeliverySettingsRepository> _mockDeliverySettingsRepository;
    private readonly Mock<ITenantContext> _mockTenantContext;
    private readonly Guid _tenantId = Guid.NewGuid();

    public DeleteDeliverySettingsCommandHandlerTests()
    {
        _mockDeliverySettingsRepository = new Mock<IDeliverySettingsRepository>();
        _mockTenantContext = new Mock<ITenantContext>();
        _mockTenantContext.Setup(x => x.TenantId).Returns(_tenantId);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldDeactivateDeliverySettings()
    {
        var settings = DeliverySettings.Create(
            _tenantId, DeliveryFeeType.Fixed, IDelivery.Domain.Common.ValueObjects.Money.Create(5.00m).Value).Value;
        _mockDeliverySettingsRepository.Setup(x => x.GetByTenantIdAsync(_tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(settings);

        var command = new DeleteDeliverySettingsCommand();

        var handler = new DeleteDeliverySettingsCommandHandler(
            _mockDeliverySettingsRepository.Object,
            _mockTenantContext.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        settings.IsActive.Should().BeFalse();
        _mockDeliverySettingsRepository.Verify(x => x.UpdateAsync(settings, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithoutTenant_ShouldFail()
    {
        _mockTenantContext.Setup(x => x.TenantId).Returns((Guid?)null);

        var command = new DeleteDeliverySettingsCommand();

        var handler = new DeleteDeliverySettingsCommandHandler(
            _mockDeliverySettingsRepository.Object,
            _mockTenantContext.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("DeliverySettings.TenantRequired");
    }

    [Fact]
    public async Task Handle_WhenSettingsNotFound_ShouldFail()
    {
        _mockDeliverySettingsRepository.Setup(x => x.GetByTenantIdAsync(_tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DeliverySettings?)null);

        var command = new DeleteDeliverySettingsCommand();

        var handler = new DeleteDeliverySettingsCommandHandler(
            _mockDeliverySettingsRepository.Object,
            _mockTenantContext.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("DeliverySettings.NotFound");
    }
}
