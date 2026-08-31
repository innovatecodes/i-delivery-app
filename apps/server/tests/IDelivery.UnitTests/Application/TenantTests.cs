using IDelivery.Application.Commands.Tenants;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.Application.Abstractions.Security;
using IDelivery.Domain.Tenants.ValueObjects;
using IDelivery.Domain.Common.ValueObjects;
using FluentAssertions;
using Moq;
using System;
using System.Threading;
using Xunit;

namespace IDelivery.UnitTests.Application; 

public class CreateTenantCommandHandlerTests
{
    private readonly Mock<IPasswordHasher> _mockPasswordHasher;
    private readonly Mock<ISecureTokenGenerator> _mockTokenGenerator;
    private readonly Mock<ITenantRepository> _mockTenantRepository;

    public CreateTenantCommandHandlerTests()
    {
        _mockPasswordHasher = new Mock<IPasswordHasher>();
        _mockTokenGenerator = new Mock<ISecureTokenGenerator>();
        _mockTenantRepository = new Mock<ITenantRepository>();
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateTenant()
    {
        var command = new CreateTenantCommand(
            "Test Restaurant",
            "test-restaurant",
            "A test restaurant",
            "https://example.com/logo.png",
            new Address("Rua A", "123", "Apto 10", "Centro", "São Paulo", "SP", ZipCode.Create("01234-567")),
            Email.Create("test@restaurant.com"),
            PhoneNumber.Create("(11) 99999-9999"),
            PhoneNumber.Create("(11) 98888-8888"));

        var handler = new CreateTenantCommandHandler(
            _mockTenantRepository.Object,
            _mockPasswordHasher.Object,
            _mockTokenGenerator.Object);

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
            _mockTenantRepository.Object,
            _mockPasswordHasher.Object,
            _mockTokenGenerator.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Tenant.SlugAlreadyExists");
    }

    [Fact]
    public async Task Handle_WithEmptyName_ShouldFail()
    {
        var command = new CreateTenantCommand("", "test-restaurant");

        var handler = new CreateTenantCommandHandler(
            _mockTenantRepository.Object,
            _mockPasswordHasher.Object,
            _mockTokenGenerator.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }
}

public class CreateTenantCommandValidatorTests
{
    [Fact]
    public void Validate_WithValidData_ShouldPass()
    {
        var command = new CreateTenantCommand(
            "Test Restaurant",
            "test-restaurant",
            "A test restaurant",
            "https://example.com/logo.png",
            new Address("Rua A", "123", "Apto 10", "Centro", "São Paulo", "SP", ZipCode.Create("01234-567")),
            Email.Create("test@restaurant.com"),
            PhoneNumber.Create("(11) 99999-9999"),
            PhoneNumber.Create("(11) 98888-8888"));

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