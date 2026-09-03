using IDelivery.Application.Abstractions.Events;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.Application.Abstractions.Security;
using IDelivery.Domain.Users.Events;

namespace IDelivery.Application.Events.Handlers;

public sealed class UserPasswordResetRequestedDomainEventHandler : IDomainEventHandler<UserPasswordResetRequestedDomainEvent>
{
    private readonly ISecureTokenGenerator _tokenGenerator;
    private readonly ITokenHasher _tokenHasher;
    private readonly IUserRepository _userRepository;
    private readonly IDomainEventDispatcher _eventDispatcher;
    private readonly IUnitOfWork _unitOfWork;

    public UserPasswordResetRequestedDomainEventHandler(
        ISecureTokenGenerator tokenGenerator,
        ITokenHasher tokenHasher,
        IUserRepository userRepository,
        IDomainEventDispatcher domainEventDispatcher,
        IUnitOfWork unitOfWork)
    {
        _tokenGenerator = tokenGenerator;
        _tokenHasher = tokenHasher;
        _userRepository = userRepository;
        _eventDispatcher = domainEventDispatcher;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UserPasswordResetRequestedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdTrackedAsync(domainEvent.UserId, cancellationToken);

        if (user is null)
        {
            return;
        }

        var resetToken = _tokenGenerator.Generate(32);
        var resetTokenHash = _tokenHasher.Hash(resetToken);
        var resetTokenExpiresAt = DateTime.UtcNow.AddHours(1);

        user.SetResetPasswordToken(resetTokenHash, resetTokenExpiresAt);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var tokenGeneratedEvent = new UserPasswordResetTokenGeneratedDomainEvent(
            user.Id,
            user.Email.Value,
            resetToken);

        await _eventDispatcher.DispatchAsync(tokenGeneratedEvent, cancellationToken);
    }
}
