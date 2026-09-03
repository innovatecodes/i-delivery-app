using IDelivery.Application.Commands.Catalog;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.Application.Abstractions.Authentication;
using IDelivery.Domain.Catalog.Entities;
using IDelivery.Domain.Common.ValueObjects;
using FluentAssertions;
using Moq;
using System;
using System.Threading;
using Xunit;

namespace IDelivery.UnitTests.Application;

public class CreateProductCommandHandlerTests
{
    private readonly Mock<IProductRepository> _mockProductRepository;
    private readonly Mock<ITenantContext> _mockTenantContext;
    private readonly Guid _tenantId = Guid.NewGuid();

    public CreateProductCommandHandlerTests()
    {
        _mockProductRepository = new Mock<IProductRepository>();
        _mockTenantContext = new Mock<ITenantContext>();
        _mockTenantContext.Setup(x => x.TenantId).Returns(_tenantId);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateProduct()
    {
        var command = new CreateProductCommand(
            "Coca-Cola 350ml",
            5.50m,
            "BRL",
            null,
            "Refrigerante lata",
            "https://example.com/coca.png",
            1);

        var handler = new CreateProductCommandHandler(
            _mockProductRepository.Object,
            _mockTenantContext.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        _mockProductRepository.Verify(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithoutTenant_ShouldFail()
    {
        _mockTenantContext.Setup(x => x.TenantId).Returns((Guid?)null);

        var command = new CreateProductCommand(
            "Coca-Cola 350ml",
            5.50m,
            "BRL",
            null,
            null,
            null,
            0);

        var handler = new CreateProductCommandHandler(
            _mockProductRepository.Object,
            _mockTenantContext.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Product.TenantRequired");
    }

    [Fact]
    public async Task Handle_WithExistingName_ShouldFail()
    {
        _mockProductRepository.Setup(x => x.ExistsByNameAsync(
            _tenantId, "Coca-Cola", It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new CreateProductCommand(
            "Coca-Cola",
            5.50m,
            "BRL",
            null,
            null,
            null,
            0);

        var handler = new CreateProductCommandHandler(
            _mockProductRepository.Object,
            _mockTenantContext.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Product.NameAlreadyExists");
    }
}

public class UpdateProductCommandHandlerTests
{
    private readonly Mock<IProductRepository> _mockProductRepository;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _productId = Guid.NewGuid();

    public UpdateProductCommandHandlerTests()
    {
        _mockProductRepository = new Mock<IProductRepository>();
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateProduct()
    {
        var productResult = Product.Create(_tenantId, "Coca-Cola", Money.Create(5.50m, "BRL").Value, null, null, null, 0);
        Assert.True(productResult.IsSuccess);
        var product = productResult.Value;

        _mockProductRepository.Setup(x => x.GetByIdAsync(_productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var command = new UpdateProductCommand(
            _productId,
            "Coca-Cola Atualizada",
            6.00m,
            "BRL",
            Guid.NewGuid(),
            "Nova descrição",
            "https://example.com/nova.png",
            2);

        var handler = new UpdateProductCommandHandler(_mockProductRepository.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _mockProductRepository.Verify(x => x.UpdateAsync(product, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentProduct_ShouldFail()
    {
        _mockProductRepository.Setup(x => x.GetByIdAsync(_productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var command = new UpdateProductCommand(
            _productId,
            "Coca-Cola",
            5.50m,
            "BRL",
            null,
            null,
            null,
            0);

        var handler = new UpdateProductCommandHandler(_mockProductRepository.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Product.NotFound");
    }

    [Fact]
    public async Task Handle_WithExistingName_ShouldFail()
    {
        var productResult = Product.Create(_tenantId, "Coca-Cola", Money.Create(5.50m, "BRL").Value, null, null, null, 0);
        Assert.True(productResult.IsSuccess);
        var product = productResult.Value;

        _mockProductRepository.Setup(x => x.GetByIdAsync(_productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _mockProductRepository.Setup(x => x.ExistsByNameAsync(
            _tenantId, "Pepsi", _productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new UpdateProductCommand(
            _productId,
            "Pepsi",
            5.50m,
            "BRL",
            null,
            null,
            null,
            0);

        var handler = new UpdateProductCommandHandler(_mockProductRepository.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Product.NameAlreadyExists");
    }
}

public class DeleteProductCommandHandlerTests
{
    private readonly Mock<IProductRepository> _mockProductRepository;
    private readonly Guid _productId = Guid.NewGuid();

    public DeleteProductCommandHandlerTests()
    {
        _mockProductRepository = new Mock<IProductRepository>();
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldDeactivateProduct()
    {
        var productResult = Product.Create(Guid.NewGuid(), "Coca-Cola", Money.Create(5.50m, "BRL").Value, null, null, null, 0);
        Assert.True(productResult.IsSuccess);
        var product = productResult.Value;

        _mockProductRepository.Setup(x => x.GetByIdAsync(_productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var command = new DeleteProductCommand(_productId);

        var handler = new DeleteProductCommandHandler(_mockProductRepository.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        product.IsActive.Should().BeFalse();
        _mockProductRepository.Verify(x => x.UpdateAsync(product, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentProduct_ShouldFail()
    {
        _mockProductRepository.Setup(x => x.GetByIdAsync(_productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var command = new DeleteProductCommand(_productId);

        var handler = new DeleteProductCommandHandler(_mockProductRepository.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Product.NotFound");
    }
}

public class CreateProductCommandValidatorTests
{
    [Fact]
    public void Validate_WithValidData_ShouldPass()
    {
        var command = new CreateProductCommand(
            "Coca-Cola 350ml",
            5.50m,
            "BRL",
            null,
            "Refrigerante lata",
            "https://example.com/coca.png",
            1);

        var validator = new CreateProductCommandValidator();

        var result = validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyName_ShouldFail()
    {
        var command = new CreateProductCommand(
            "",
            5.50m,
            "BRL",
            null,
            null,
            null,
            0);

        var validator = new CreateProductCommandValidator();

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public void Validate_WithNegativePrice_ShouldFail()
    {
        var command = new CreateProductCommand(
            "Coca-Cola",
            -1m,
            "BRL",
            null,
            null,
            null,
            0);

        var validator = new CreateProductCommandValidator();

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public void Validate_WithEmptyCurrency_ShouldFail()
    {
        var command = new CreateProductCommand(
            "Coca-Cola",
            5.50m,
            "",
            null,
            null,
            null,
            0);

        var validator = new CreateProductCommandValidator();

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThan(0);
    }
}

public class UpdateProductCommandValidatorTests
{
    [Fact]
    public void Validate_WithValidData_ShouldPass()
    {
        var command = new UpdateProductCommand(
            Guid.NewGuid(),
            "Coca-Cola",
            5.50m,
            "BRL",
            null,
            null,
            null,
            0);

        var validator = new UpdateProductCommandValidator();

        var result = validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyId_ShouldFail()
    {
        var command = new UpdateProductCommand(
            Guid.Empty,
            "Coca-Cola",
            5.50m,
            "BRL",
            null,
            null,
            null,
            0);

        var validator = new UpdateProductCommandValidator();

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public void Validate_WithEmptyName_ShouldFail()
    {
        var command = new UpdateProductCommand(
            Guid.NewGuid(),
            "",
            5.50m,
            "BRL",
            null,
            null,
            null,
            0);

        var validator = new UpdateProductCommandValidator();

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThan(0);
    }
}

public class DeleteProductCommandValidatorTests
{
    [Fact]
    public void Validate_WithValidId_ShouldPass()
    {
        var command = new DeleteProductCommand(Guid.NewGuid());

        var validator = new DeleteProductCommandValidator();

        var result = validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyId_ShouldFail()
    {
        var command = new DeleteProductCommand(Guid.Empty);

        var validator = new DeleteProductCommandValidator();

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThan(0);
    }
}
