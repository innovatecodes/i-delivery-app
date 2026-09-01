using IDelivery.Domain.Users.Entities;
using IDelivery.Domain.Users.Enums;
using IDelivery.Domain.Roles;
using IDelivery.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;
using System;
using System.Threading;

namespace IDelivery.IntegrationTests.Infrastructure;

public class UserPersistenceTests
{
    private readonly DbContextOptions<ApplicationDbContext> _dbContextOptions;
    private readonly Guid _tenantId = Guid.NewGuid();

    public UserPersistenceTests()
    {
        _dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task Add_ValidUser_ShouldPersist()
    {
        using var context = new ApplicationDbContext(_dbContextOptions);
        var userResult = User.Create(
            "john@test.com",
            "password123!",
            "John Doe",
            Role.Customer,
            _tenantId);
        Assert.True(userResult.IsSuccess, userResult.Error.Message);
        var user = userResult.Value;

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var savedUser = await context.Users.FindAsync(user.Id);
        savedUser.Should().NotBeNull();
        savedUser.Email.Value.Should().Be("john@test.com");
        savedUser.FullName.Should().Be("John Doe");
        savedUser.Role.Should().Be(Role.Customer);
        savedUser.TenantId.Should().Be(_tenantId);
        savedUser.Status.Should().Be(UserStatus.PendingActivation);
    }

    [Fact]
    public async Task Get_ExistingUser_ShouldReturn()
    {
        using var context = new ApplicationDbContext(_dbContextOptions);
        var userResult = User.Create(
            "john@test.com",
            "password123!",
            "John Doe",
            Role.Customer,
            _tenantId);
        Assert.True(userResult.IsSuccess);
        var user = userResult.Value;
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var found = await context.Users.FirstAsync(u => u.Email.Value == "john@test.com");

        found.Should().NotBeNull();
        found.FullName.Should().Be("John Doe");
    }

    [Fact]
    public async Task Update_ExistingUser_ShouldUpdate()
    {
        using var context = new ApplicationDbContext(_dbContextOptions);
        var userResult = User.Create(
            "john@test.com",
            "password123!",
            "John Doe",
            Role.Customer,
            _tenantId);
        Assert.True(userResult.IsSuccess);
        var user = userResult.Value;
        context.Users.Add(user);
        await context.SaveChangesAsync();

        user.UpdateProfile("John Updated", "(11) 99999-9999");
        context.Entry(user).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        await context.SaveChangesAsync();

        var updated = await context.Users.FindAsync(user.Id);
        updated?.FullName.Should().Be("John Updated");
        updated?.PhoneNumber.Should().Be("(11) 99999-9999");
    }

    [Fact]
    public async Task Remove_ExistingUser_ShouldRemove()
    {
        using var context = new ApplicationDbContext(_dbContextOptions);
        var userResult = User.Create(
            "john@test.com",
            "password123!",
            "John Doe",
            Role.Customer,
            _tenantId);
        Assert.True(userResult.IsSuccess);
        var user = userResult.Value;
        context.Users.Add(user);
        await context.SaveChangesAsync();

        context.Users.Remove(user);
        await context.SaveChangesAsync();

        var exists = await context.Users.AnyAsync(u => u.Id == user.Id);
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task ActivateByAdmin_User_ShouldWork()
    {
        using var context = new ApplicationDbContext(_dbContextOptions);
        var userResult = User.Create(
            "john@test.com",
            "password123!",
            "John Doe",
            Role.Customer,
            _tenantId);
        Assert.True(userResult.IsSuccess);
        var user = userResult.Value;
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var activateResult = user.ActivateByAdmin();
        Assert.True(activateResult.IsSuccess);
        context.Entry(user).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        await context.SaveChangesAsync();

        var activated = await context.Users.FindAsync(user.Id);
        activated?.Status.Should().Be(UserStatus.Active);
        activated?.ActivatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Deactivate_User_ShouldWork()
    {
        using var context = new ApplicationDbContext(_dbContextOptions);
        var userResult = User.Create(
            "john@test.com",
            "password123!",
            "John Doe",
            Role.Customer,
            _tenantId);
        Assert.True(userResult.IsSuccess);
        var user = userResult.Value;
        user.ActivateByAdmin();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var deactivateResult = user.Deactivate();
        Assert.True(deactivateResult.IsSuccess);
        context.Entry(user).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        await context.SaveChangesAsync();

        var deactivated = await context.Users.FindAsync(user.Id);
        deactivated?.Status.Should().Be(UserStatus.Inactive);
    }

    [Fact]
    public async Task Delete_User_ShouldWork()
    {
        using var context = new ApplicationDbContext(_dbContextOptions);
        var userResult = User.Create(
            "john@test.com",
            "password123!",
            "John Doe",
            Role.Customer,
            _tenantId);
        Assert.True(userResult.IsSuccess);
        var user = userResult.Value;
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var deleteResult = user.Delete();
        Assert.True(deleteResult.IsSuccess);
        context.Entry(user).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        await context.SaveChangesAsync();

        var deleted = await context.Users.FindAsync(user.Id);
        deleted?.Status.Should().Be(UserStatus.Deleted);
    }

    [Fact]
    public async Task ChangeRole_User_ShouldWork()
    {
        using var context = new ApplicationDbContext(_dbContextOptions);
        var userResult = User.Create(
            "john@test.com",
            "password123!",
            "John Doe",
            Role.Customer,
            _tenantId);
        Assert.True(userResult.IsSuccess);
        var user = userResult.Value;
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var changeRoleResult = user.ChangeRole(Role.Delivery, _tenantId);
        Assert.True(changeRoleResult.IsSuccess);
        context.Entry(user).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        await context.SaveChangesAsync();

        var updated = await context.Users.FindAsync(user.Id);
        updated?.Role.Should().Be(Role.Delivery);
        updated?.TenantId.Should().Be(_tenantId);
    }

    [Fact]
    public async Task GetByTenantId_MultipleUsers_ShouldReturnCorrectOrder()
    {
        using var context = new ApplicationDbContext(_dbContextOptions);
        var tenantId2 = Guid.NewGuid();

        var user1 = User.Create("john@test.com", "password123!", "John Doe", Role.Customer, _tenantId);
        var user2 = User.Create("jane@test.com", "password123!", "Jane Doe", Role.Customer, _tenantId);
        var user3 = User.Create("bob@test.com", "password123!", "Bob Smith", Role.Customer, tenantId2);

        Assert.True(user1.IsSuccess && user2.IsSuccess && user3.IsSuccess);

        context.Users.AddRange(user1.Value, user2.Value, user3.Value);
        await context.SaveChangesAsync();

        var tenantUsers = await context.Users
            .Where(u => u.TenantId == _tenantId)
            .OrderBy(u => u.FullName)
            .ToListAsync();

        tenantUsers.Should().HaveCount(2);
        tenantUsers[0].FullName.Should().Be("Jane Doe");
        tenantUsers[1].FullName.Should().Be("John Doe");
    }

    [Fact]
    public async Task GetByRole_MultipleUsers_ShouldReturnCorrectOrder()
    {
        using var context = new ApplicationDbContext(_dbContextOptions);

        var user1 = User.Create("john@test.com", "password123!", "John Doe", Role.Customer, _tenantId);
        var user2 = User.Create("jane@test.com", "password123!", "Jane Doe", Role.Delivery, _tenantId);
        var user3 = User.Create("bob@test.com", "password123!", "Bob Smith", Role.Customer, _tenantId);

        Assert.True(user1.IsSuccess && user2.IsSuccess && user3.IsSuccess);

        context.Users.AddRange(user1.Value, user2.Value, user3.Value);
        await context.SaveChangesAsync();

        var customerUsers = await context.Users
            .Where(u => u.Role == Role.Customer)
            .OrderBy(u => u.FullName)
            .ToListAsync();

        customerUsers.Should().HaveCount(2);
        customerUsers[0].FullName.Should().Be("Bob Smith");
        customerUsers[1].FullName.Should().Be("John Doe");
    }
}
