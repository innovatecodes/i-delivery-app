using IDelivery.Domain.Catalog.Entities;
using IDelivery.Domain.Common.ValueObjects;
using IDelivery.Domain.Common.Exceptions;
using FluentAssertions;

namespace IDelivery.UnitTests.Domain;

public class ProductTests
{
    private readonly Guid _tenantId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidData_ShouldCreateProduct()
    {
        var result = Product.Create(
            _tenantId,
            "Coca-Cola 350ml",
            Money.Create(5.99m, "BRL").Value,
            null,
            "Refrigerante lata",
            null,
            1);

        result.IsSuccess.Should().BeTrue();
        var product = result.Value;
        product.Id.Should().NotBeEmpty();
        product.TenantId.Should().Be(_tenantId);
        product.Name.Should().Be("Coca-Cola 350ml");
        product.Price.Amount.Should().Be(5.99m);
        product.Price.Currency.Should().Be("BRL");
        product.Description.Should().Be("Refrigerante lata");
        product.IsActive.Should().BeTrue();
        product.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public void Create_WithEmptyTenantId_ShouldFail()
    {
        var result = Product.Create(
            Guid.Empty,
            "Coca-Cola",
            Money.Create(5.99m, "BRL").Value);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Create_WithEmptyName_ShouldFail()
    {
        var result = Product.Create(
            _tenantId,
            "",
            Money.Create(5.99m, "BRL").Value);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Create_WithNameTooLong_ShouldFail()
    {
        var result = Product.Create(
            _tenantId,
            new string('A', 201),
            Money.Create(5.99m, "BRL").Value);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Create_WithNegativePrice_ShouldFail()
    {
        var result = Money.Create(-1m, "BRL");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Money.NegativeAmount");
    }

    [Fact]
    public void Create_WithEmptyCurrency_ShouldFail()
    {
        var result = Money.Create(5.99m, "");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Money.EmptyCurrency");
    }

    [Fact]
    public void UpdateDetails_WithValidData_ShouldUpdate()
    {
        var createResult = Product.Create(_tenantId, "Original", Money.Create(10m, "BRL").Value);
        Assert.True(createResult.IsSuccess);
        var product = createResult.Value;

        var updateResult = product.UpdateDetails(
            "Updated",
            "New description",
            Money.Create(15.99m, "BRL").Value,
            null,
            null,
            2);

        updateResult.IsSuccess.Should().BeTrue();
        product.Name.Should().Be("Updated");
        product.Description.Should().Be("New description");
        product.Price.Amount.Should().Be(15.99m);
        product.SortOrder.Should().Be(2);
    }

    [Fact]
    public void UpdateDetails_WithEmptyName_ShouldFail()
    {
        var createResult = Product.Create(_tenantId, "Original", Money.Create(10m, "BRL").Value);
        Assert.True(createResult.IsSuccess);
        var product = createResult.Value;

        var updateResult = product.UpdateDetails("", null, Money.Create(10m, "BRL").Value, null, null, 0);
        updateResult.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Activate_WhenInactive_ShouldActivate()
    {
        var createResult = Product.Create(_tenantId, "Test", Money.Create(10m, "BRL").Value);
        Assert.True(createResult.IsSuccess);
        var product = createResult.Value;

        product.Deactivate();
        product.IsActive.Should().BeFalse();

        var activateResult = product.Activate();
        activateResult.IsSuccess.Should().BeTrue();
        product.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_WhenActive_ShouldDeactivate()
    {
        var createResult = Product.Create(_tenantId, "Test", Money.Create(10m, "BRL").Value);
        Assert.True(createResult.IsSuccess);
        var product = createResult.Value;

        var deactivateResult = product.Deactivate();
        deactivateResult.IsSuccess.Should().BeTrue();
        product.IsActive.Should().BeFalse();
    }

    [Fact]
    public void MarkAsAvailable_WhenUnavailable_ShouldMarkAsAvailable()
    {
        var createResult = Product.Create(_tenantId, "Test", Money.Create(10m, "BRL").Value);
        Assert.True(createResult.IsSuccess);
        var product = createResult.Value;

        product.MarkAsUnavailable();
        product.IsAvailable.Should().BeFalse();

        var markResult = product.MarkAsAvailable();
        markResult.IsSuccess.Should().BeTrue();
        product.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public void MarkAsUnavailable_WhenAvailable_ShouldMarkAsUnavailable()
    {
        var createResult = Product.Create(_tenantId, "Test", Money.Create(10m, "BRL").Value);
        Assert.True(createResult.IsSuccess);
        var product = createResult.Value;

        var markResult = product.MarkAsUnavailable();
        markResult.IsSuccess.Should().BeTrue();
        product.IsAvailable.Should().BeFalse();
    }

    [Fact]
    public void ChangeCategory_WithValidCategoryId_ShouldChange()
    {
        var createResult = Product.Create(_tenantId, "Test", Money.Create(10m, "BRL").Value);
        Assert.True(createResult.IsSuccess);
        var product = createResult.Value;

        var categoryId = Guid.NewGuid();
        var changeResult = product.ChangeCategory(categoryId);
        changeResult.IsSuccess.Should().BeTrue();
        product.CategoryId.Should().Be(categoryId);
    }
}
