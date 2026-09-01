using IDelivery.Domain.Catalog.Entities;
using IDelivery.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;
using System;
using System.Threading;

namespace IDelivery.IntegrationTests.Infrastructure;

public class ProductPersistenceTests
{
    private readonly DbContextOptions<ApplicationDbContext> _dbContextOptions;
    private readonly Guid _tenantId = Guid.NewGuid();

    public ProductPersistenceTests()
    {
        _dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task Add_ValidProduct_ShouldPersist()
    {
        using var context = new ApplicationDbContext(_dbContextOptions);
        var productResult = Product.Create(
            _tenantId,
            "Coca-Cola 350ml",
            5.50m,
            "BRL",
            null,
            "Refrigerante lata",
            "https://example.com/coca.png",
            1);
        Assert.True(productResult.IsSuccess, productResult.Error.Message);
        var product = productResult.Value;

        context.Products.Add(product);
        await context.SaveChangesAsync();

        var savedProduct = await context.Products.FindAsync(product.Id);
        savedProduct.Should().NotBeNull();
        savedProduct.Name.Should().Be("Coca-Cola 350ml");
        savedProduct.Price.Amount.Should().Be(5.50m);
        savedProduct.Price.Currency.Should().Be("BRL");
        savedProduct.Description.Should().Be("Refrigerante lata");
        savedProduct.ImageUrl.Should().Be("https://example.com/coca.png");
        savedProduct.TenantId.Should().Be(_tenantId);
    }

    [Fact]
    public async Task Get_ExistingProduct_ShouldReturn()
    {
        using var context = new ApplicationDbContext(_dbContextOptions);
        var productResult = Product.Create(
            _tenantId,
            "Coca-Cola",
            5.50m,
            "BRL",
            null,
            null,
            null,
            0);
        Assert.True(productResult.IsSuccess);
        var product = productResult.Value;
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var found = await context.Products.FirstAsync(p => p.Name == "Coca-Cola" && p.TenantId == _tenantId);

        found.Should().NotBeNull();
        found.Name.Should().Be("Coca-Cola");
    }

    [Fact]
    public async Task Update_ExistingProduct_ShouldUpdate()
    {
        using var context = new ApplicationDbContext(_dbContextOptions);
        var productResult = Product.Create(
            _tenantId,
            "Coca-Cola",
            5.50m,
            "BRL",
            null,
            null,
            null,
            0);
        Assert.True(productResult.IsSuccess);
        var product = productResult.Value;
        context.Products.Add(product);
        await context.SaveChangesAsync();

        product.UpdateDetails("Coca-Cola Atualizada", "Nova descrição", 6.00m, "BRL", null, "https://example.com/nova.png", 2);
        context.Entry(product).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        await context.SaveChangesAsync();

        var updated = await context.Products.FindAsync(product.Id);
        updated?.Name.Should().Be("Coca-Cola Atualizada");
        updated?.Description.Should().Be("Nova descrição");
        updated?.Price.Amount.Should().Be(6.00m);
        updated?.SortOrder.Should().Be(2);
    }

    [Fact]
    public async Task Remove_ExistingProduct_ShouldRemove()
    {
        using var context = new ApplicationDbContext(_dbContextOptions);
        var productResult = Product.Create(
            _tenantId,
            "To Be Removed",
            5.50m,
            "BRL",
            null,
            null,
            null,
            0);
        Assert.True(productResult.IsSuccess);
        var product = productResult.Value;
        context.Products.Add(product);
        await context.SaveChangesAsync();

        context.Products.Remove(product);
        await context.SaveChangesAsync();

        var exists = await context.Products.AnyAsync(p => p.Id == product.Id);
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task ActivateAndDeactivate_Product_ShouldWork()
    {
        using var context = new ApplicationDbContext(_dbContextOptions);
        var productResult = Product.Create(
            _tenantId,
            "Coca-Cola",
            5.50m,
            "BRL",
            null,
            null,
            null,
            0);
        Assert.True(productResult.IsSuccess);
        var product = productResult.Value;
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var deactivateResult = product.Deactivate();
        Assert.True(deactivateResult.IsSuccess);
        context.Entry(product).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        await context.SaveChangesAsync();

        var deactivated = await context.Products.FindAsync(product.Id);
        deactivated?.IsActive.Should().BeFalse();

        var activateResult = product.Activate();
        Assert.True(activateResult.IsSuccess);
        context.Entry(product).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        await context.SaveChangesAsync();

        var activated = await context.Products.FindAsync(product.Id);
        activated?.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task MarkAsAvailableAndUnavailable_Product_ShouldWork()
    {
        using var context = new ApplicationDbContext(_dbContextOptions);
        var productResult = Product.Create(
            _tenantId,
            "Coca-Cola",
            5.50m,
            "BRL",
            null,
            null,
            null,
            0);
        Assert.True(productResult.IsSuccess);
        var product = productResult.Value;
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var unavailableResult = product.MarkAsUnavailable();
        Assert.True(unavailableResult.IsSuccess);
        context.Entry(product).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        await context.SaveChangesAsync();

        var unavailable = await context.Products.FindAsync(product.Id);
        unavailable?.IsAvailable.Should().BeFalse();

        var availableResult = product.MarkAsAvailable();
        Assert.True(availableResult.IsSuccess);
        context.Entry(product).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        await context.SaveChangesAsync();

        var available = await context.Products.FindAsync(product.Id);
        available?.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public async Task ChangeCategory_Product_ShouldWork()
    {
        using var context = new ApplicationDbContext(_dbContextOptions);
        var categoryId = Guid.NewGuid();
        var productResult = Product.Create(
            _tenantId,
            "Coca-Cola",
            5.50m,
            "BRL",
            null,
            null,
            null,
            0);
        Assert.True(productResult.IsSuccess);
        var product = productResult.Value;
        context.Products.Add(product);
        await context.SaveChangesAsync();

        product.ChangeCategory(categoryId);
        context.Entry(product).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        await context.SaveChangesAsync();

        var updated = await context.Products.FindAsync(product.Id);
        updated?.CategoryId.Should().Be(categoryId);
    }

    [Fact]
    public async Task GetByCategoryId_MultipleProducts_ShouldReturnCorrectOrder()
    {
        using var context = new ApplicationDbContext(_dbContextOptions);
        var categoryId = Guid.NewGuid();
        var categoryId2 = Guid.NewGuid();

        var prod1 = Product.Create(_tenantId, "Coca-Cola", 5.50m, "BRL", categoryId, null, null, 2);
        var prod2 = Product.Create(_tenantId, "Pepsi", 5.00m, "BRL", categoryId, null, null, 1);
        var prod3 = Product.Create(_tenantId, "Guaraná", 4.50m, "BRL", categoryId2, null, null, 0);

        Assert.True(prod1.IsSuccess && prod2.IsSuccess && prod3.IsSuccess);

        context.Products.AddRange(prod1.Value, prod2.Value, prod3.Value);
        await context.SaveChangesAsync();

        var categoryProducts = await context.Products
            .Where(p => p.CategoryId == categoryId)
            .OrderBy(p => p.SortOrder)
            .ToListAsync();

        categoryProducts.Should().HaveCount(2);
        categoryProducts[0].Name.Should().Be("Pepsi");
        categoryProducts[1].Name.Should().Be("Coca-Cola");
    }
}
