using IDelivery.Application.Commands.Tenants;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.Domain.Tenants.Entities;
using FluentAssertions;
using Moq;
using System;
using System.Threading;
using Xunit;

namespace IDelivery.UnitTests.Application;

public class CreateTenantCommandHandlerTests
{
    private readonly Mock<ITenantRepository> _mockTenantRepository;

    public CreateTenantCommandHandlerTests()
    {
        _mockTenantRepository = new Mock<ITenantRepository>();
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateTenant()
    {
        var command = new CreateTenantCommand(
            "Test Restaurant",
            "test-restaurant",
            "A test restaurant",
            "https://example.com/logo.png");

        var handler = new CreateTenantCommandHandler(
            _mockTenantRepository.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_WithExistingSlug_ShouldFail()
    {
        var command = new CreateTenantCommand(
            "Test Restaurant",
            "existing-slug",
            "A test restaurant",
            "https://example.com/logo.png");

        _mockTenantRepository.Setup(x => x.ExistsBySlugAsync("existing-slug", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new CreateTenantCommandHandler(
            _mockTenantRepository.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Tenant.SlugAlreadyExists");
    }

    [Fact]
    public async Task Handle_WithEmptyName_ShouldFail()
    {
        var command = new CreateTenantCommand("", "test-restaurant");

        var handler = new CreateTenantCommandHandler(
            _mockTenantRepository.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }
}

public class UpdateTenantCommandHandlerTests
{
    private readonly Mock<ITenantRepository> _mockTenantRepository;

    public UpdateTenantCommandHandlerTests()
    {
        _mockTenantRepository = new Mock<ITenantRepository>();
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateTenant()
    {
        var createResult = Tenant.Create("Original Name", "original-slug");
        Assert.True(createResult.IsSuccess);
        var tenant = createResult.Value;

        _mockTenantRepository.Setup(x => x.GetByIdAsync(tenant.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        var command = new UpdateTenantCommand(
            tenant.Id,
            "Updated Name",
            "Updated description",
            "https://example.com/new.png");

        var handler = new UpdateTenantCommandHandler(_mockTenantRepository.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithNonExistentTenant_ShouldFail()
    {
        var command = new UpdateTenantCommand(
            Guid.NewGuid(),
            "Updated Name",
            null,
            null);

        var handler = new UpdateTenantCommandHandler(_mockTenantRepository.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Tenant.NotFound");
    }
}

public class UpdateTenantCommandValidatorTests
{
    [Fact]
    public void Validate_WithValidData_ShouldPass()
    {
        var command = new UpdateTenantCommand(
            Guid.NewGuid(),
            "Test Restaurant",
            "Description",
            "https://example.com/logo.png");

        var validator = new UpdateTenantCommandValidator();

        var result = validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyId_ShouldFail()
    {
        var command = new UpdateTenantCommand(
            Guid.Empty,
            "Test Restaurant",
            null,
            null);

        var validator = new UpdateTenantCommandValidator();

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public void Validate_WithEmptyName_ShouldFail()
    {
        var command = new UpdateTenantCommand(
            Guid.NewGuid(),
            "",
            null,
            null);

        var validator = new UpdateTenantCommandValidator();

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThan(0);
    }
}

public class DeleteTenantCommandHandlerTests
{
    private readonly Mock<ITenantRepository> _mockTenantRepository;

    public DeleteTenantCommandHandlerTests()
    {
        _mockTenantRepository = new Mock<ITenantRepository>();
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldBlockTenant()
    {
        var createResult = Tenant.Create("Test Tenant", "test-tenant");
        Assert.True(createResult.IsSuccess);
        var tenant = createResult.Value;

        _mockTenantRepository.Setup(x => x.GetByIdAsync(tenant.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        var command = new DeleteTenantCommand(tenant.Id);

        var handler = new DeleteTenantCommandHandler(_mockTenantRepository.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        tenant.Status.Should().Be(IDelivery.Domain.Tenants.Enums.TenantStatus.Blocked);
    }

    [Fact]
    public async Task Handle_WithNonExistentTenant_ShouldFail()
    {
        var command = new DeleteTenantCommand(Guid.NewGuid());

        var handler = new DeleteTenantCommandHandler(_mockTenantRepository.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Tenant.NotFound");
    }
}

public class DeleteTenantCommandValidatorTests
{
    [Fact]
    public void Validate_WithValidId_ShouldPass()
    {
        var command = new DeleteTenantCommand(Guid.NewGuid());

        var validator = new DeleteTenantCommandValidator();

        var result = validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyId_ShouldFail()
    {
        var command = new DeleteTenantCommand(Guid.Empty);

        var validator = new DeleteTenantCommandValidator();

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThan(0);
    }
}

public class BlockTenantCommandHandlerTests
{
    private readonly Mock<ITenantRepository> _mockTenantRepository;

    public BlockTenantCommandHandlerTests()
    {
        _mockTenantRepository = new Mock<ITenantRepository>();
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldBlockTenant()
    {
        var createResult = Tenant.Create("Test Tenant", "test-tenant");
        Assert.True(createResult.IsSuccess);
        var tenant = createResult.Value;

        _mockTenantRepository.Setup(x => x.GetByIdAsync(tenant.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        var command = new BlockTenantCommand(tenant.Id);

        var handler = new BlockTenantCommandHandler(_mockTenantRepository.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        tenant.Status.Should().Be(IDelivery.Domain.Tenants.Enums.TenantStatus.Blocked);
    }

    [Fact]
    public async Task Handle_WhenAlreadyBlocked_ShouldFail()
    {
        var createResult = Tenant.Create("Test Tenant", "test-tenant");
        Assert.True(createResult.IsSuccess);
        var tenant = createResult.Value;
        tenant.Block();

        _mockTenantRepository.Setup(x => x.GetByIdAsync(tenant.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        var command = new BlockTenantCommand(tenant.Id);

        var handler = new BlockTenantCommandHandler(_mockTenantRepository.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Tenant.AlreadyBlocked");
    }

    [Fact]
    public async Task Handle_WithNonExistentTenant_ShouldFail()
    {
        var command = new BlockTenantCommand(Guid.NewGuid());

        var handler = new BlockTenantCommandHandler(_mockTenantRepository.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Tenant.NotFound");
    }
}

public class ActivateTenantCommandHandlerTests
{
    private readonly Mock<ITenantRepository> _mockTenantRepository;

    public ActivateTenantCommandHandlerTests()
    {
        _mockTenantRepository = new Mock<ITenantRepository>();
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldActivateTenant()
    {
        var createResult = Tenant.Create("Test Tenant", "test-tenant");
        Assert.True(createResult.IsSuccess);
        var tenant = createResult.Value;
        tenant.Block();

        _mockTenantRepository.Setup(x => x.GetByIdAsync(tenant.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        var command = new ActivateTenantCommand(tenant.Id);

        var handler = new ActivateTenantCommandHandler(_mockTenantRepository.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        tenant.Status.Should().Be(IDelivery.Domain.Tenants.Enums.TenantStatus.Active);
    }

    [Fact]
    public async Task Handle_WhenAlreadyActive_ShouldFail()
    {
        var createResult = Tenant.Create("Test Tenant", "test-tenant");
        Assert.True(createResult.IsSuccess);
        var tenant = createResult.Value;

        _mockTenantRepository.Setup(x => x.GetByIdAsync(tenant.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        var command = new ActivateTenantCommand(tenant.Id);

        var handler = new ActivateTenantCommandHandler(_mockTenantRepository.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Tenant.AlreadyActive");
    }

    [Fact]
    public async Task Handle_WithNonExistentTenant_ShouldFail()
    {
        var command = new ActivateTenantCommand(Guid.NewGuid());

        var handler = new ActivateTenantCommandHandler(_mockTenantRepository.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Tenant.NotFound");
    }
}

public class CreateTenantCommandValidatorTests
{
    [Fact]
    public void Validate_WithValidData_ShouldPass()
    {
        var command = new CreateTenantCommand(
            "Test Restaurant",
            "test-restaurant");

        var validator = new CreateTenantCommandValidator();

        var result = validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyName_ShouldFail()
    {
        var command = new CreateTenantCommand("", "test-restaurant");

        var validator = new CreateTenantCommandValidator();

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public void Validate_WithEmptySlug_ShouldFail()
    {
        var command = new CreateTenantCommand("", "");

        var validator = new CreateTenantCommandValidator();

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThan(0);
    }
}
