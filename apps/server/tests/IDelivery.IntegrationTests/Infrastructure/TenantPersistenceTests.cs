using IDelivery.Domain.Tenants.Entities;
using IDelivery.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;
using System;
using System.Threading;
using System.Collections.Generic;

namespace IDelivery.IntegrationTests.Infrastructure;

public class TenantPersistenceTests
{
    private readonly DbContextOptions<ApplicationDbContext> _dbContextOptions;

    public TenantPersistenceTests()
    {
        _dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task Add_ValidTenant_ShouldPersist()
    {
        using var context = new ApplicationDbContext(_dbContextOptions);
        var tenantResult = Tenant.Create(
            "Test Restaurant",
            "test-restaurant");
        Assert.True(tenantResult.IsSuccess, tenantResult.Error.Message);
        var tenant = tenantResult.Value;

        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();

        var savedTenant = await context.Tenants.FindAsync(tenant.Id);
        savedTenant.Should().NotBeNull();
        savedTenant.Name.Should().Be("Test Restaurant");
    }

    [Fact]
    public async Task Get_ExistingTenant_ShouldReturn()
    {
        using var context = new ApplicationDbContext(_dbContextOptions);
        var tenantResult = Tenant.Create(
            "Test Restaurant",
            "test-restaurant");
        Assert.True(tenantResult.IsSuccess);
        var tenant = tenantResult.Value;
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();

        var found = await context.Tenants.FirstAsync(t => t.Slug == "test-restaurant");

        found.Should().NotBeNull();
        found.Name.Should().Be("Test Restaurant");
    }

    [Fact]
    public async Task Update_ExistingTenant_ShouldUpdate()
    {
        using var context = new ApplicationDbContext(_dbContextOptions);
        var tenantResult = Tenant.Create(
            "Old Name",
            "test-restaurant");
        Assert.True(tenantResult.IsSuccess);
        var tenant = tenantResult.Value;
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();

        tenant.UpdateDetails("New Name", "New description", "https://example.com/new.png");
        context.Entry(tenant).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        await context.SaveChangesAsync();

        var updated = await context.Tenants.FindAsync(tenant.Id);
        updated?.Name.Should().Be("New Name");
        updated?.Description.Should().Be("New description");
        updated?.LogoUrl.Should().Be("https://example.com/new.png");
    }

    [Fact]
    public async Task Remove_ExistingTenant_ShouldRemove()
    {
        using var context = new ApplicationDbContext(_dbContextOptions);
        var tenantResult = Tenant.Create(
            "To Be Removed",
            "to-remove");
        Assert.True(tenantResult.IsSuccess);
        var tenant = tenantResult.Value;
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();

        context.Tenants.Remove(tenant);
        await context.SaveChangesAsync();

        var exists = await context.Tenants.AnyAsync(t => t.Id == tenant.Id);
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task BlockAndActivate_Tenant_ShouldWork()
    {
        using var context = new ApplicationDbContext(_dbContextOptions);
        var tenantResult = Tenant.Create(
            "Test Restaurant",
            "test-restaurant");
        Assert.True(tenantResult.IsSuccess);
        var tenant = tenantResult.Value;
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();

        // Tenant starts as Active, so test Block first
        var blockResult = tenant.Block();
        Assert.True(blockResult.IsSuccess);
        context.Entry(tenant).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        await context.SaveChangesAsync();

        var blocked = await context.Tenants.FindAsync(tenant.Id);
        blocked?.Status.Should().Be(IDelivery.Domain.Tenants.Enums.TenantStatus.Blocked);

        // Then Activate
        var activateResult = tenant.Activate();
        Assert.True(activateResult.IsSuccess);
        context.Entry(tenant).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        await context.SaveChangesAsync();

        var activated = await context.Tenants.FindAsync(tenant.Id);
        activated?.Status.Should().Be(IDelivery.Domain.Tenants.Enums.TenantStatus.Active);
    }
}