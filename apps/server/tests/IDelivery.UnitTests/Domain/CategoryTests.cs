using IDelivery.Domain.Catalog.Entities;
using Xunit;
using FluentAssertions;

namespace IDelivery.UnitTests.Domain;

public class CategoryTests
{
    private readonly Guid _tenantId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidData_ShouldCreateCategory()
    {
        var result = Category.Create(
            _tenantId,
            "Bebidas",
            "Bebidas geladas",
            null,
            1);

        result.IsSuccess.Should().BeTrue();
        var category = result.Value;
        category.Id.Should().NotBeEmpty();
        category.TenantId.Should().Be(_tenantId);
        category.Name.Should().Be("Bebidas");
        category.Description.Should().Be("Bebidas geladas");
        category.SortOrder.Should().Be(1);
        category.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_WithEmptyTenantId_ShouldFail()
    {
        var result = Category.Create(
            Guid.Empty,
            "Bebidas");

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Create_WithEmptyName_ShouldFail()
    {
        var result = Category.Create(
            _tenantId,
            "");

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Create_WithNameTooLong_ShouldFail()
    {
        var result = Category.Create(
            _tenantId,
            new string('A', 101));

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void UpdateDetails_WithValidData_ShouldUpdate()
    {
        var createResult = Category.Create(_tenantId, "Original");
        Assert.True(createResult.IsSuccess);
        var category = createResult.Value;

        var updateResult = category.UpdateDetails("Updated", "New description", null, 2);
        updateResult.IsSuccess.Should().BeTrue();
        category.Name.Should().Be("Updated");
        category.Description.Should().Be("New description");
        category.SortOrder.Should().Be(2);
    }

    [Fact]
    public void UpdateDetails_WithEmptyName_ShouldFail()
    {
        var createResult = Category.Create(_tenantId, "Original");
        Assert.True(createResult.IsSuccess);
        var category = createResult.Value;

        var updateResult = category.UpdateDetails("", null, null, 0);
        updateResult.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Activate_WhenInactive_ShouldActivate()
    {
        var createResult = Category.Create(_tenantId, "Test");
        Assert.True(createResult.IsSuccess);
        var category = createResult.Value;

        category.Deactivate();
        category.IsActive.Should().BeFalse();

        var activateResult = category.Activate();
        activateResult.IsSuccess.Should().BeTrue();
        category.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Activate_WhenAlreadyActive_ShouldFail()
    {
        var createResult = Category.Create(_tenantId, "Test");
        Assert.True(createResult.IsSuccess);
        var category = createResult.Value;

        var activateResult = category.Activate();
        activateResult.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_WhenActive_ShouldDeactivate()
    {
        var createResult = Category.Create(_tenantId, "Test");
        Assert.True(createResult.IsSuccess);
        var category = createResult.Value;

        var deactivateResult = category.Deactivate();
        deactivateResult.IsSuccess.Should().BeTrue();
        category.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_ShouldFail()
    {
        var createResult = Category.Create(_tenantId, "Test");
        Assert.True(createResult.IsSuccess);
        var category = createResult.Value;

        category.Deactivate();
        var deactivateResult = category.Deactivate();
        deactivateResult.IsFailure.Should().BeTrue();
    }
}
