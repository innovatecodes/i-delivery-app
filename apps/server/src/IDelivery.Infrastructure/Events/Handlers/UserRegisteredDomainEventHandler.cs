using IDelivery.Application.Abstractions.Events;
using IDelivery.Application.Abstractions.Messaging;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.Application.Abstractions.Security;
using IDelivery.Domain.Users.Events;

namespace IDelivery.Infrastructure.Events.Handlers;

public sealed class UserRegisteredDomainEventHandler : IDomainEventHandler<UserRegisteredDomainEvent>
{
    private readonly ISecureTokenGenerator _tokenGenerator;
    private readonly ITokenHasher _tokenHasher;
    private readonly IEmailService _emailService;
    private readonly IUserRepository _userRepository;

    public UserRegisteredDomainEventHandler(
        ISecureTokenGenerator tokenGenerator,
        ITokenHasher tokenHasher,
        IEmailService emailService,
        IUserRepository userRepository)
    {
        _tokenGenerator = tokenGenerator;
        _tokenHasher = tokenHasher;
        _emailService = emailService;
        _userRepository = userRepository;
    }

    public async Task Handle(UserRegisteredDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(domainEvent.UserId, cancellationToken);
        
        if (user is null)
        {
            return;
        }

        var activationToken = _tokenGenerator.Generate(32);
        var activationTokenHash = _tokenHasher.Hash(activationToken);
        var activationTokenExpiresAt = DateTime.UtcNow.AddHours(24);

        user.SetActivationToken(activationTokenHash, activationTokenExpiresAt);

        var activationLink = $"https://app.idelivery.com/activate?token={activationToken}&email={Uri.EscapeDataString(user.Email.Value)}";
        await _emailService.SendActivationEmailAsync(user.Email.Value, activationLink, cancellationToken);
    }
}