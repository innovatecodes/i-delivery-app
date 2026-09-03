using IDelivery.Application.Commands.Catalog;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.Application.Abstractions.Authentication;
using IDelivery.Domain.Catalog.Entities;
using IDelivery.Domain.Tenants.Enums;
using FluentAssertions;
using Moq;
using System;
using System.Threading;
using Xunit;

namespace IDelivery.UnitTests.Application;

public class CreateCategoryCommandHandlerTests
{
    private readonly Mock<ICategoryRepository> _mockCategoryRepository;
    private readonly Mock<ITenantContext> _mockTenantContext;
    private readonly Guid _tenantId = Guid.NewGuid();

    public CreateCategoryCommandHandlerTests()
    {
        _mockCategoryRepository = new Mock<ICategoryRepository>();
        _mockTenantContext = new Mock<ITenantContext>();
        _mockTenantContext.Setup(x => x.TenantId).Returns(_tenantId);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateCategory()
    {
        var command = new CreateCategoryCommand(
            "Bebidas",
            "Bebidas geladas",
            "https://example.com/bebidas.png",
            1);

        var handler = new CreateCategoryCommandHandler(
            _mockCategoryRepository.Object,
            _mockTenantContext.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        _mockCategoryRepository.Verify(x => x.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithoutTenant_ShouldFail()
    {
        _mockTenantContext.Setup(x => x.TenantId).Returns((Guid?)null);

        var command = new CreateCategoryCommand(
            "Bebidas",
            null,
            null,
            0);

        var handler = new CreateCategoryCommandHandler(
            _mockCategoryRepository.Object,
            _mockTenantContext.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Category.TenantRequired");
    }

    [Fact]
    public async Task Handle_WithExistingName_ShouldFail()
    {
        _mockCategoryRepository.Setup(x => x.ExistsByNameAsync(
            _tenantId, "Bebidas", It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new CreateCategoryCommand(
            "Bebidas",
            null,
            null,
            0);

        var handler = new CreateCategoryCommandHandler(
            _mockCategoryRepository.Object,
            _mockTenantContext.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Category.NameAlreadyExists");
    }
}

public class UpdateCategoryCommandHandlerTests
{
    private readonly Mock<ICategoryRepository> _mockCategoryRepository;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _categoryId = Guid.NewGuid();

    public UpdateCategoryCommandHandlerTests()
    {
        _mockCategoryRepository = new Mock<ICategoryRepository>();
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateCategory()
    {
        var categoryResult = Category.Create(_tenantId, "Bebidas", null, null, 0);
        Assert.True(categoryResult.IsSuccess);
        var category = categoryResult.Value;

        _mockCategoryRepository.Setup(x => x.GetByIdAsync(_categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var command = new UpdateCategoryCommand(
            _categoryId,
            "Bebidas Atualizadas",
            "Nova descrição",
            "https://example.com/nova.png",
            2);

        var handler = new UpdateCategoryCommandHandler(_mockCategoryRepository.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _mockCategoryRepository.Verify(x => x.UpdateAsync(category, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentCategory_ShouldFail()
    {
        _mockCategoryRepository.Setup(x => x.GetByIdAsync(_categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        var command = new UpdateCategoryCommand(
            _categoryId,
            "Bebidas",
            null,
            null,
            0);

        var handler = new UpdateCategoryCommandHandler(_mockCategoryRepository.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Category.NotFound");
    }

    [Fact]
    public async Task Handle_WithExistingName_ShouldFail()
    {
        var categoryResult = Category.Create(_tenantId, "Bebidas", null, null, 0);
        Assert.True(categoryResult.IsSuccess);
        var category = categoryResult.Value;

        _mockCategoryRepository.Setup(x => x.GetByIdAsync(_categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _mockCategoryRepository.Setup(x => x.ExistsByNameAsync(
            _tenantId, "Outro Nome", _categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new UpdateCategoryCommand(
            _categoryId,
            "Outro Nome",
            null,
            null,
            0);

        var handler = new UpdateCategoryCommandHandler(_mockCategoryRepository.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Category.NameAlreadyExists");
    }
}

public class DeleteCategoryCommandHandlerTests
{
    private readonly Mock<ICategoryRepository> _mockCategoryRepository;
    private readonly Guid _categoryId = Guid.NewGuid();

    public DeleteCategoryCommandHandlerTests()
    {
        _mockCategoryRepository = new Mock<ICategoryRepository>();
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldDeactivateCategory()
    {
        var categoryResult = Category.Create(Guid.NewGuid(), "Bebidas", null, null, 0);
        Assert.True(categoryResult.IsSuccess);
        var category = categoryResult.Value;

        _mockCategoryRepository.Setup(x => x.GetByIdAsync(_categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var command = new DeleteCategoryCommand(_categoryId);

        var handler = new DeleteCategoryCommandHandler(_mockCategoryRepository.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        category.IsActive.Should().BeFalse();
        _mockCategoryRepository.Verify(x => x.UpdateAsync(category, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentCategory_ShouldFail()
    {
        _mockCategoryRepository.Setup(x => x.GetByIdAsync(_categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        var command = new DeleteCategoryCommand(_categoryId);

        var handler = new DeleteCategoryCommandHandler(_mockCategoryRepository.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Category.NotFound");
    }
}

public class CreateCategoryCommandValidatorTests
{
    [Fact]
    public void Validate_WithValidData_ShouldPass()
    {
        var command = new CreateCategoryCommand(
            "Bebidas",
            "Bebidas geladas",
            "https://example.com/bebidas.png",
            1);

        var validator = new CreateCategoryCommandValidator();

        var result = validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyName_ShouldFail()
    {
        var command = new CreateCategoryCommand(
            "",
            null,
            null,
            0);

        var validator = new CreateCategoryCommandValidator();

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public void Validate_WithNegativeSortOrder_ShouldFail()
    {
        var command = new CreateCategoryCommand(
            "Bebidas",
            null,
            null,
            -1);

        var validator = new CreateCategoryCommandValidator();

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThan(0);
    }
}

public class UpdateCategoryCommandValidatorTests
{
    [Fact]
    public void Validate_WithValidData_ShouldPass()
    {
        var command = new UpdateCategoryCommand(
            Guid.NewGuid(),
            "Bebidas",
            "Descrição",
            "https://example.com/bebidas.png",
            1);

        var validator = new UpdateCategoryCommandValidator();

        var result = validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyId_ShouldFail()
    {
        var command = new UpdateCategoryCommand(
            Guid.Empty,
            "Bebidas",
            null,
            null,
            0);

        var validator = new UpdateCategoryCommandValidator();

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public void Validate_WithEmptyName_ShouldFail()
    {
        var command = new UpdateCategoryCommand(
            Guid.NewGuid(),
            "",
            null,
            null,
            0);

        var validator = new UpdateCategoryCommandValidator();

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThan(0);
    }
}

public class DeleteCategoryCommandValidatorTests
{
    [Fact]
    public void Validate_WithValidId_ShouldPass()
    {
        var command = new DeleteCategoryCommand(Guid.NewGuid());

        var validator = new DeleteCategoryCommandValidator();

        var result = validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyId_ShouldFail()
    {
        var command = new DeleteCategoryCommand(Guid.Empty);

        var validator = new DeleteCategoryCommandValidator();

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThan(0);
    }
}
