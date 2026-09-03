using FluentAssertions;
using IDelivery.Application.Abstractions.Events;
using IDelivery.Domain.Common.DomainEvents;
using IDelivery.Domain.Common.Entities;
using IDelivery.Domain.Common.ValueObjects;
using IDelivery.Domain.Roles;
using IDelivery.Domain.Users.Entities;
using IDelivery.Domain.Users.Events;
using IDelivery.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using Xunit;

namespace IDelivery.IntegrationTests.Events;

public class DomainEventPipelineTests
{
    private readonly Mock<IDomainEventDispatcher> _mockDispatcher;

    public DomainEventPipelineTests()
    {
        _mockDispatcher = new Mock<IDomainEventDispatcher>();
    }

    private ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new ApplicationDbContext(options, _mockDispatcher.Object);
    }

    private static User CreateUser()
    {
        var email = Email.Create("john@test.com").Value;
        var userResult = User.Create(email, "password123!", "John Doe", Role.Customer, null);
        return userResult.Value;
    }

    [Fact]
    public async Task SaveChanges_ShouldDispatchDomainEvents()
    {
        using var context = CreateContext();
        var user = CreateUser();

        context.Users.Add(user);
        await context.SaveChangesAsync();

        _mockDispatcher.Verify(
            x => x.DispatchAsync(
                It.IsAny<UserRegisteredDomainEvent>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SaveChanges_ShouldClearEventsAfterDispatch()
    {
        using var context = CreateContext();
        var user = CreateUser();

        context.Users.Add(user);
        await context.SaveChangesAsync();

        user.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task SaveChanges_WithMultipleAggregates_ShouldDispatchAllEvents()
    {
        using var context = CreateContext();
        var email1 = Email.Create("john@test.com").Value;
        var user1 = User.Create(email1, "password123!", "John Doe", Role.Customer, null).Value;

        var email2 = Email.Create("jane@test.com").Value;
        var user2 = User.Create(email2, "password123!", "Jane Doe", Role.Customer, null).Value;

        context.Users.Add(user1);
        context.Users.Add(user2);
        await context.SaveChangesAsync();

        _mockDispatcher.Verify(
            x => x.DispatchAsync(
                It.IsAny<UserRegisteredDomainEvent>(),
                It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task SaveChanges_DuringDispatch_ShouldNotRedispatch()
    {
        using var context = CreateContext();
        var user = CreateUser();

        var dispatchCount = 0;
        _mockDispatcher.Setup(x => x.DispatchAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()))
            .Callback(() => dispatchCount++);

        context.Users.Add(user);
        await context.SaveChangesAsync();

        dispatchCount.Should().Be(1);
    }

    [Fact]
    public async Task SaveChanges_PersistenceFailure_ShouldNotSilentlyIgnoreDispatchFailure()
    {
        _mockDispatcher.Setup(x => x.DispatchAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Dispatch failed"));

        using var context = CreateContext();
        var user = CreateUser();

        context.Users.Add(user);

        var act = async () => await context.SaveChangesAsync();

        await act.Should().ThrowAsync<Exception>().WithMessage("Dispatch failed");
    }

    [Fact]
    public async Task SaveChanges_HandlerCallingSaveChanges_ShouldNotRedispatch()
    {
        using var context = CreateContext();
        var user = CreateUser();

        var dispatchCount = 0;
        _mockDispatcher.Setup(x => x.DispatchAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()))
            .Callback(() => dispatchCount++);

        context.Users.Add(user);
        await context.SaveChangesAsync();

        dispatchCount.Should().Be(1);
        user.DomainEvents.Should().BeEmpty();
    }
}
