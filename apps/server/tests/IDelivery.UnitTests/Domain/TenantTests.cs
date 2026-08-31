using IDelivery.Domain.Tenants.Entities;
using IDelivery.Domain.Tenants.Enums;
using Xunit;
using FluentAssertions;

namespace IDelivery.UnitTests.Domain;

public class TenantTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateTenant()
    {
        var result = Tenant.Create(
            "Test Restaurant",
            "test-restaurant");

        result.IsSuccess.Should().BeTrue();
        var tenant = result.Value;
        tenant.Id.Should().NotBeEmpty();
        tenant.Name.Should().Be("Test Restaurant");
        tenant.Slug.Should().Be("test-restaurant");
        tenant.Description.Should().BeNull();
        tenant.LogoUrl.Should().BeNull();
        tenant.Status.Should().Be(TenantStatus.Active);
    }

    [Fact]
    public void Create_WithEmptyName_ShouldFail()
    {
        var result = Tenant.Create(
            "",
            "test-restaurant");

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Create_WithEmptySlug_ShouldFail()
    {
        var result = Tenant.Create(
            "Test Restaurant",
            "");

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void UpdateDetails_WithValidData_ShouldUpdate()
    {
        var createResult = Tenant.Create("Original Name", "original-slug");
        Assert.True(createResult.IsSuccess);
        var tenant = createResult.Value;

        var updateResult = tenant.UpdateDetails("Updated Name", "Updated description", "https://example.com/new.png");
        updateResult.IsSuccess.Should().BeTrue();
        tenant.Name.Should().Be("Updated Name");
        tenant.Description.Should().Be("Updated description");
        tenant.LogoUrl.Should().Be("https://example.com/new.png");
    }

    [Fact]
    public void UpdateDetails_WithEmptyName_ShouldFail()
    {
        var createResult = Tenant.Create("Original Name", "original-slug");
        Assert.True(createResult.IsSuccess);
        var tenant = createResult.Value;

        var updateResult = tenant.UpdateDetails("", "Description", "https://example.com.png");
        updateResult.IsFailure.Should().BeTrue();
    }
}