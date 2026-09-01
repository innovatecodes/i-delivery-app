using IDelivery.Domain.Catalog.Entities;
using IDelivery.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;
using System;
using System.Threading;

namespace IDelivery.IntegrationTests.Infrastructure;

public class CategoryPersistenceTests
{
    private readonly DbContextOptions<ApplicationDbContext> _dbContextOptions;
    private readonly Guid _tenantId = Guid.NewGuid();

    public CategoryPersistenceTests()
    {
        _dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task Add_ValidCategory_ShouldPersist()
    {
        using var context = new ApplicationDbContext(_dbContextOptions);
        var categoryResult = Category.Create(
            _tenantId,
            "Bebidas",
            "Bebidas geladas",
            "https://example.com/bebidas.png",
            1);
        Assert.True(categoryResult.IsSuccess, categoryResult.Error.Message);
        var category = categoryResult.Value;

        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var savedCategory = await context.Categories.FindAsync(category.Id);
        savedCategory.Should().NotBeNull();
        savedCategory.Name.Should().Be("Bebidas");
        savedCategory.Description.Should().Be("Bebidas geladas");
        savedCategory.ImageUrl.Should().Be("https://example.com/bebidas.png");
        savedCategory.SortOrder.Should().Be(1);
        savedCategory.TenantId.Should().Be(_tenantId);
    }

    [Fact]
    public async Task Get_ExistingCategory_ShouldReturn()
    {
        using var context = new ApplicationDbContext(_dbContextOptions);
        var categoryResult = Category.Create(
            _tenantId,
            "Bebidas",
            null,
            null,
            0);
        Assert.True(categoryResult.IsSuccess);
        var category = categoryResult.Value;
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var found = await context.Categories.FirstAsync(c => c.Name == "Bebidas" && c.TenantId == _tenantId);

        found.Should().NotBeNull();
        found.Name.Should().Be("Bebidas");
    }

    [Fact]
    public async Task Update_ExistingCategory_ShouldUpdate()
    {
        using var context = new ApplicationDbContext(_dbContextOptions);
        var categoryResult = Category.Create(
            _tenantId,
            "Bebidas",
            null,
            null,
            0);
        Assert.True(categoryResult.IsSuccess);
        var category = categoryResult.Value;
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        category.UpdateDetails("Bebidas Atualizadas", "Nova descrição", "https://example.com/nova.png", 2);
        context.Entry(category).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        await context.SaveChangesAsync();

        var updated = await context.Categories.FindAsync(category.Id);
        updated?.Name.Should().Be("Bebidas Atualizadas");
        updated?.Description.Should().Be("Nova descrição");
        updated?.ImageUrl.Should().Be("https://example.com/nova.png");
        updated?.SortOrder.Should().Be(2);
    }

    [Fact]
    public async Task Remove_ExistingCategory_ShouldRemove()
    {
        using var context = new ApplicationDbContext(_dbContextOptions);
        var categoryResult = Category.Create(
            _tenantId,
            "To Be Removed",
            null,
            null,
            0);
        Assert.True(categoryResult.IsSuccess);
        var category = categoryResult.Value;
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        context.Categories.Remove(category);
        await context.SaveChangesAsync();

        var exists = await context.Categories.AnyAsync(c => c.Id == category.Id);
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task ActivateAndDeactivate_Category_ShouldWork()
    {
        using var context = new ApplicationDbContext(_dbContextOptions);
        var categoryResult = Category.Create(
            _tenantId,
            "Bebidas",
            null,
            null,
            0);
        Assert.True(categoryResult.IsSuccess);
        var category = categoryResult.Value;
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var deactivateResult = category.Deactivate();
        Assert.True(deactivateResult.IsSuccess);
        context.Entry(category).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        await context.SaveChangesAsync();

        var deactivated = await context.Categories.FindAsync(category.Id);
        deactivated?.IsActive.Should().BeFalse();

        var activateResult = category.Activate();
        Assert.True(activateResult.IsSuccess);
        context.Entry(category).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        await context.SaveChangesAsync();

        var activated = await context.Categories.FindAsync(category.Id);
        activated?.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetByTenantId_MultipleCategories_ShouldReturnCorrectOrder()
    {
        using var context = new ApplicationDbContext(_dbContextOptions);
        var tenantId2 = Guid.NewGuid();

        var cat1 = Category.Create(_tenantId, "Bebidas", null, null, 2);
        var cat2 = Category.Create(_tenantId, "Lanches", null, null, 1);
        var cat3 = Category.Create(tenantId2, "Outro Tenant", null, null, 0);

        Assert.True(cat1.IsSuccess && cat2.IsSuccess && cat3.IsSuccess);

        context.Categories.AddRange(cat1.Value, cat2.Value, cat3.Value);
        await context.SaveChangesAsync();

        var tenantCategories = await context.Categories
            .Where(c => c.TenantId == _tenantId)
            .OrderBy(c => c.SortOrder)
            .ToListAsync();

        tenantCategories.Should().HaveCount(2);
        tenantCategories[0].Name.Should().Be("Lanches");
        tenantCategories[1].Name.Should().Be("Bebidas");
    }
}
