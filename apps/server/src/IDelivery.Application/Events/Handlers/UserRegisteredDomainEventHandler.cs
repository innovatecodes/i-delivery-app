using IDelivery.Application.Abstractions.Events;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.Application.Abstractions.Security;

using IDelivery.Domain.Users.Events;

namespace IDelivery.Application.Events.Handlers;

public sealed class UserRegisteredDomainEventHandler : IDomainEventHandler<UserRegisteredDomainEvent>
{
    private readonly ISecureTokenGenerator _tokenGenerator;
    private readonly ITokenHasher _tokenHasher;
    private readonly IUserRepository _userRepository;
    private readonly IDomainEventDispatcher _eventDispatcher;
    private readonly IUnitOfWork _unitOfWork;

    public UserRegisteredDomainEventHandler(
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

    public async Task Handle(UserRegisteredDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdTrackedAsync(domainEvent.UserId, cancellationToken);

        if (user is null)
        {
            return;
        }

        var activationToken = _tokenGenerator.Generate(32);
        var activationTokenHash = _tokenHasher.Hash(activationToken);
        var activationTokenExpiresAt = DateTime.UtcNow.AddHours(24);

        user.SetActivationToken(activationTokenHash, activationTokenExpiresAt);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Dispara o evento específico informando que o token foi gerado para este usuário
        var tokenGeneratedEvent = new UserActivationTokenGeneratedDomainEvent(
            user.Id,
            user.Email.Value,
            activationToken);

        await _eventDispatcher.DispatchAsync(tokenGeneratedEvent, cancellationToken);
    }
}

