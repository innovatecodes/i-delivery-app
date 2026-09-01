using IDelivery.Domain.Tenants.Entities;
using IDelivery.Domain.Tenants.Enums;
using IDelivery.Domain.Common.ValueObjects;
using IDelivery.Domain.Tenants.ValueObjects;
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
    public void Create_WithNameTooLong_ShouldFail()
    {
        var longName = new string('A', 201);
        var result = Tenant.Create(
            longName,
            "test-restaurant");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Tenant.NameTooLong");
    }

    [Fact]
    public void Create_WithSlugTooLong_ShouldFail()
    {
        var longSlug = new string('a', 101);
        var result = Tenant.Create(
            "Test Restaurant",
            longSlug);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Tenant.SlugTooLong");
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

    [Fact]
    public void UpdateDetails_WithNameTooLong_ShouldFail()
    {
        var createResult = Tenant.Create("Original Name", "original-slug");
        Assert.True(createResult.IsSuccess);
        var tenant = createResult.Value;

        var longName = new string('A', 201);
        var updateResult = tenant.UpdateDetails(longName, "Description", null);
        updateResult.IsFailure.Should().BeTrue();
        updateResult.Error.Code.Should().Be("Tenant.NameTooLong");
    }

    [Fact]
    public void UpdateAddress_WithValidAddress_ShouldUpdate()
    {
        var createResult = Tenant.Create("Test Tenant", "test-tenant");
        Assert.True(createResult.IsSuccess);
        var tenant = createResult.Value;

        var address = new Address("Rua A", "123", "Apto 10", "Centro", "São Paulo", "SP", ZipCode.Create("01234-567"));
        var result = tenant.UpdateAddress(address);

        result.IsSuccess.Should().BeTrue();
        tenant.Address.Should().NotBeNull();
        tenant.Address.Street.Should().Be("Rua A");
    }

    [Fact]
    public void UpdateContactInfo_WithValidData_ShouldUpdate()
    {
        var createResult = Tenant.Create("Test Tenant", "test-tenant");
        Assert.True(createResult.IsSuccess);
        var tenant = createResult.Value;

        var email = Email.Create("test@restaurant.com");
        var phone = PhoneNumber.Create("(11) 99999-9999");
        var whatsApp = PhoneNumber.Create("(11) 98888-8888");

        var result = tenant.UpdateContactInfo(email, phone, whatsApp);

        result.IsSuccess.Should().BeTrue();
        tenant.Email.Value.Should().Be("test@restaurant.com");
        tenant.Phone.ToString().Should().Be("(11) 99999-9999");
        tenant.WhatsApp.ToString().Should().Be("(11) 98888-8888");
    }

    [Fact]
    public void Activate_WhenBlocked_ShouldActivate()
    {
        var createResult = Tenant.Create("Test Tenant", "test-tenant");
        Assert.True(createResult.IsSuccess);
        var tenant = createResult.Value;
        tenant.Block();

        var result = tenant.Activate();

        result.IsSuccess.Should().BeTrue();
        tenant.Status.Should().Be(TenantStatus.Active);
    }

    [Fact]
    public void Activate_WhenAlreadyActive_ShouldFail()
    {
        var createResult = Tenant.Create("Test Tenant", "test-tenant");
        Assert.True(createResult.IsSuccess);
        var tenant = createResult.Value;

        var result = tenant.Activate();

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Tenant.AlreadyActive");
    }

    [Fact]
    public void Block_WhenActive_ShouldBlock()
    {
        var createResult = Tenant.Create("Test Tenant", "test-tenant");
        Assert.True(createResult.IsSuccess);
        var tenant = createResult.Value;

        var result = tenant.Block();

        result.IsSuccess.Should().BeTrue();
        tenant.Status.Should().Be(TenantStatus.Blocked);
    }

    [Fact]
    public void Block_WhenAlreadyBlocked_ShouldFail()
    {
        var createResult = Tenant.Create("Test Tenant", "test-tenant");
        Assert.True(createResult.IsSuccess);
        var tenant = createResult.Value;
        tenant.Block();

        var result = tenant.Block();

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Tenant.AlreadyBlocked");
    }
}