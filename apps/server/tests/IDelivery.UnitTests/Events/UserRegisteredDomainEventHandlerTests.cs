using FluentAssertions;
using IDelivery.Application.Abstractions.Events;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.Application.Abstractions.Security;
using IDelivery.Application.Events.Handlers;
using IDelivery.Domain.Common.DomainEvents;
using IDelivery.Domain.Common.ValueObjects;
using IDelivery.Domain.Roles;
using IDelivery.Domain.Users.Entities;
using IDelivery.Domain.Users.Events;
using Moq;
using Xunit;

namespace IDelivery.UnitTests.Events;

public class UserRegisteredDomainEventHandlerTests
{
    private readonly Mock<ISecureTokenGenerator> _mockTokenGenerator;
    private readonly Mock<ITokenHasher> _mockTokenHasher;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IDomainEventDispatcher> _mockEventDispatcher;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly UserRegisteredDomainEventHandler _handler;

    public UserRegisteredDomainEventHandlerTests()
    {
        _mockTokenGenerator = new Mock<ISecureTokenGenerator>();
        _mockTokenHasher = new Mock<ITokenHasher>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockEventDispatcher = new Mock<IDomainEventDispatcher>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _handler = new UserRegisteredDomainEventHandler(
            _mockTokenGenerator.Object,
            _mockTokenHasher.Object,
            _mockUserRepository.Object,
            _mockEventDispatcher.Object,
            _mockUnitOfWork.Object);
    }

    [Fact]
    public async Task Handle_ShouldGenerateActivationToken()
    {
        var user = CreateUser();
        var domainEvent = new UserRegisteredDomainEvent(user.Id, user.Email.Value, user.FullName, user.Role);

        _mockUserRepository.Setup(x => x.GetByIdTrackedAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mockTokenGenerator.Setup(x => x.Generate(32)).Returns("activation-token");
        _mockTokenHasher.Setup(x => x.Hash("activation-token")).Returns("activation-token-hash");

        await _handler.Handle(domainEvent, CancellationToken.None);

        _mockTokenGenerator.Verify(x => x.Generate(32), Times.Once);
        _mockTokenHasher.Verify(x => x.Hash("activation-token"), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSetActivationTokenOnUser()
    {
        var user = CreateUser();
        var domainEvent = new UserRegisteredDomainEvent(user.Id, user.Email.Value, user.FullName, user.Role);

        _mockUserRepository.Setup(x => x.GetByIdTrackedAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mockTokenGenerator.Setup(x => x.Generate(32)).Returns("activation-token");
        _mockTokenHasher.Setup(x => x.Hash("activation-token")).Returns("activation-token-hash");

        await _handler.Handle(domainEvent, CancellationToken.None);

        user.ActivationTokenHash.Should().Be("activation-token-hash");
        user.ActivationTokenExpiresAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ShouldPersistChanges()
    {
        var user = CreateUser();
        var domainEvent = new UserRegisteredDomainEvent(user.Id, user.Email.Value, user.FullName, user.Role);

        _mockUserRepository.Setup(x => x.GetByIdTrackedAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mockTokenGenerator.Setup(x => x.Generate(32)).Returns("activation-token");
        _mockTokenHasher.Setup(x => x.Hash("activation-token")).Returns("activation-token-hash");

        await _handler.Handle(domainEvent, CancellationToken.None);

        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldDispatchActivationTokenGeneratedEvent()
    {
        var user = CreateUser();
        var domainEvent = new UserRegisteredDomainEvent(user.Id, user.Email.Value, user.FullName, user.Role);

        _mockUserRepository.Setup(x => x.GetByIdTrackedAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mockTokenGenerator.Setup(x => x.Generate(32)).Returns("activation-token");
        _mockTokenHasher.Setup(x => x.Hash("activation-token")).Returns("activation-token-hash");

        await _handler.Handle(domainEvent, CancellationToken.None);

        _mockEventDispatcher.Verify(
            x => x.DispatchAsync(
                It.Is<UserActivationTokenGeneratedDomainEvent>(e =>
                    e.UserId == user.Id &&
                    e.Email == user.Email.Value &&
                    e.ActivationToken == "activation-token"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldGenerateOnlyOneActivationToken()
    {
        var user = CreateUser();
        var domainEvent = new UserRegisteredDomainEvent(user.Id, user.Email.Value, user.FullName, user.Role);

        _mockUserRepository.Setup(x => x.GetByIdTrackedAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mockTokenGenerator.Setup(x => x.Generate(32)).Returns("activation-token");
        _mockTokenHasher.Setup(x => x.Hash("activation-token")).Returns("activation-token-hash");

        await _handler.Handle(domainEvent, CancellationToken.None);

        _mockTokenGenerator.Verify(x => x.Generate(32), Times.Once);
        user.ActivationTokenHash.Should().Be("activation-token-hash");
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldNotThrow()
    {
        var domainEvent = new UserRegisteredDomainEvent(Guid.NewGuid(), "test@test.com", "Test User", Role.Customer);

        _mockUserRepository.Setup(x => x.GetByIdTrackedAsync(domainEvent.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var act = async () => await _handler.Handle(domainEvent, CancellationToken.None);

        await act.Should().NotThrowAsync();
        _mockTokenGenerator.Verify(x => x.Generate(It.IsAny<int>()), Times.Never);
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
