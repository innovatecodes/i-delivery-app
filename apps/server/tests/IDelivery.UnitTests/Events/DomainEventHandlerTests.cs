using FluentAssertions;
using IDelivery.Application.Abstractions.Events;
using IDelivery.Application.Abstractions.Messaging;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.Application.Abstractions.Security;
using IDelivery.Application.Common.Models;
using IDelivery.Application.Events.Handlers;
using IDelivery.Domain.Common.DomainEvents;
using IDelivery.Domain.Common.ValueObjects;
using IDelivery.Domain.Roles;
using IDelivery.Domain.Users.Entities;
using IDelivery.Domain.Users.Events;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace IDelivery.UnitTests.Events;

public class UserActivationTokenGeneratedDomainEventHandlerTests
{
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly Mock<ILogger<UserActivationTokenGeneratedDomainEventHandler>> _mockLogger;
    private readonly UserActivationTokenGeneratedDomainEventHandler _handler;

    public UserActivationTokenGeneratedDomainEventHandlerTests()
    {
        _mockNotificationService = new Mock<INotificationService>();
        _mockLogger = new Mock<ILogger<UserActivationTokenGeneratedDomainEventHandler>>();
        _handler = new UserActivationTokenGeneratedDomainEventHandler(
            _mockNotificationService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ShouldCallNotifyAsyncWithCorrectPayload()
    {
        var domainEvent = new UserActivationTokenGeneratedDomainEvent(Guid.NewGuid(), "test@test.com", "activation-token");

        await _handler.Handle(domainEvent, CancellationToken.None);

        _mockNotificationService.Verify(
            x => x.NotifyAsync<UserActivationPayload>(
                domainEvent.Email,
                It.Is<UserActivationPayload>(p => p.Token == domainEvent.ActivationToken),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNotificationFails_ShouldNotThrow()
    {
        var domainEvent = new UserActivationTokenGeneratedDomainEvent(Guid.NewGuid(), "test@test.com", "activation-token");

        _mockNotificationService
            .Setup(x => x.NotifyAsync<UserActivationPayload>(
                It.IsAny<string>(),
                It.IsAny<UserActivationPayload>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Notification failed"));

        var act = async () => await _handler.Handle(domainEvent, CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_WhenNotificationFails_ShouldLogError()
    {
        var domainEvent = new UserActivationTokenGeneratedDomainEvent(Guid.NewGuid(), "test@test.com", "activation-token");

        _mockNotificationService
            .Setup(x => x.NotifyAsync<UserActivationPayload>(
                It.IsAny<string>(),
                It.IsAny<UserActivationPayload>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Notification failed"));

        await _handler.Handle(domainEvent, CancellationToken.None);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(domainEvent.UserId.ToString())),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}

public class UserPasswordResetRequestedDomainEventHandlerTests
{
    private readonly Mock<ISecureTokenGenerator> _mockTokenGenerator;
    private readonly Mock<ITokenHasher> _mockTokenHasher;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IDomainEventDispatcher> _mockEventDispatcher;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly UserPasswordResetRequestedDomainEventHandler _handler;

    public UserPasswordResetRequestedDomainEventHandlerTests()
    {
        _mockTokenGenerator = new Mock<ISecureTokenGenerator>();
        _mockTokenHasher = new Mock<ITokenHasher>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockEventDispatcher = new Mock<IDomainEventDispatcher>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _handler = new UserPasswordResetRequestedDomainEventHandler(
            _mockTokenGenerator.Object,
            _mockTokenHasher.Object,
            _mockUserRepository.Object,
            _mockEventDispatcher.Object,
            _mockUnitOfWork.Object);
    }

    [Fact]
    public async Task Handle_ShouldGenerateTokenHashAndPersist()
    {
        var user = CreateUser();
        var domainEvent = new UserPasswordResetRequestedDomainEvent(user.Id, user.Email.Value);

        _mockUserRepository.Setup(x => x.GetByIdTrackedAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mockTokenGenerator.Setup(x => x.Generate(32)).Returns("reset-token");
        _mockTokenHasher.Setup(x => x.Hash("reset-token")).Returns("reset-token-hash");

        await _handler.Handle(domainEvent, CancellationToken.None);

        _mockTokenGenerator.Verify(x => x.Generate(32), Times.Once);
        _mockTokenHasher.Verify(x => x.Hash("reset-token"), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldDispatchPasswordResetTokenGeneratedEvent()
    {
        var user = CreateUser();
        var domainEvent = new UserPasswordResetRequestedDomainEvent(user.Id, user.Email.Value);

        _mockUserRepository.Setup(x => x.GetByIdTrackedAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mockTokenGenerator.Setup(x => x.Generate(32)).Returns("reset-token");
        _mockTokenHasher.Setup(x => x.Hash("reset-token")).Returns("reset-token-hash");

        await _handler.Handle(domainEvent, CancellationToken.None);

        _mockEventDispatcher.Verify(
            x => x.DispatchAsync(
                It.Is<UserPasswordResetTokenGeneratedDomainEvent>(e =>
                    e.UserId == user.Id &&
                    e.Email == user.Email.Value &&
                    e.ResetToken == "reset-token"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldNotThrow()
    {
        var domainEvent = new UserPasswordResetRequestedDomainEvent(Guid.NewGuid(), "test@test.com");

        _mockUserRepository.Setup(x => x.GetByIdTrackedAsync(domainEvent.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var act = async () => await _handler.Handle(domainEvent, CancellationToken.None);

        await act.Should().NotThrowAsync();
        _mockTokenGenerator.Verify(x => x.Generate(It.IsAny<int>()), Times.Never);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockEventDispatcher.Verify(
            x => x.DispatchAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static User CreateUser()
    {
        var email = Email.Create("john@test.com").Value;
        var userResult = User.Create(email, "password123!", "John Doe", Role.Customer, null);
        return userResult.Value;
    }
}

public class UserPasswordResetTokenGeneratedDomainEventHandlerTests
{
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly Mock<ILogger<UserPasswordResetTokenGeneratedDomainEventHandler>> _mockLogger;
    private readonly UserPasswordResetTokenGeneratedDomainEventHandler _handler;

    public UserPasswordResetTokenGeneratedDomainEventHandlerTests()
    {
        _mockNotificationService = new Mock<INotificationService>();
        _mockLogger = new Mock<ILogger<UserPasswordResetTokenGeneratedDomainEventHandler>>();
        _handler = new UserPasswordResetTokenGeneratedDomainEventHandler(
            _mockNotificationService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ShouldCallNotifyAsyncWithCorrectPayload()
    {
        var domainEvent = new UserPasswordResetTokenGeneratedDomainEvent(Guid.NewGuid(), "test@test.com", "reset-token");

        await _handler.Handle(domainEvent, CancellationToken.None);

        _mockNotificationService.Verify(
            x => x.NotifyAsync<UserPasswordResetPayload>(
                domainEvent.Email,
                It.Is<UserPasswordResetPayload>(p => p.Token == domainEvent.ResetToken),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNotificationFails_ShouldNotThrow()
    {
        var domainEvent = new UserPasswordResetTokenGeneratedDomainEvent(Guid.NewGuid(), "test@test.com", "reset-token");

        _mockNotificationService
            .Setup(x => x.NotifyAsync<UserPasswordResetPayload>(
                It.IsAny<string>(),
                It.IsAny<UserPasswordResetPayload>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Notification failed"));

        var act = async () => await _handler.Handle(domainEvent, CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_WhenNotificationFails_ShouldLogError()
    {
        var domainEvent = new UserPasswordResetTokenGeneratedDomainEvent(Guid.NewGuid(), "test@test.com", "reset-token");

        _mockNotificationService
            .Setup(x => x.NotifyAsync<UserPasswordResetPayload>(
                It.IsAny<string>(),
                It.IsAny<UserPasswordResetPayload>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Notification failed"));

        await _handler.Handle(domainEvent, CancellationToken.None);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(domainEvent.UserId.ToString())),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private static User CreateUser()
    {
        var email = Email.Create("john@test.com").Value;
        var userResult = User.Create(email, "password123!", "John Doe", Role.Customer, null);
        return userResult.Value;
    }
}
