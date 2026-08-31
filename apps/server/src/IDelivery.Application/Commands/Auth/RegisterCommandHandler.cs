using IDelivery.Application.Abstractions.CQRS;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.Application.Abstractions.Security;
using IDelivery.Application.Abstractions.Services;
using IDelivery.Domain.Roles;
using IDelivery.Domain.Users.Entities;
using IDelivery.SharedKernel.Common.Result;

namespace IDelivery.Application.Commands.Auth;

public sealed class RegisterCommandHandler : ICommandHandler<RegisterCommand, Guid>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ISecureTokenGenerator _tokenGenerator;
    private readonly ITokenHasher _tokenHasher;
    private readonly IEmailService _emailService;

    public RegisterCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ISecureTokenGenerator tokenGenerator,
        ITokenHasher tokenHasher,
        IEmailService emailService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
        _tokenHasher = tokenHasher;
        _emailService = emailService;
    }

    public async Task<Result<Guid>> Handle(RegisterCommand command, CancellationToken cancellationToken = default)
    {
        if (await _userRepository.ExistsByEmailAsync(command.Email, cancellationToken))
        {
            return Result.Failure<Guid>(new Error("User.EmailAlreadyExists", "Email já está em uso"));
        }

        var passwordHash = _passwordHasher.Hash(command.Password);

        var userResult = User.Create(
            command.Email,
            passwordHash,
            command.FullName,
            Role.Customer,
            null,
            command.PhoneNumber);

        if (userResult.IsFailure)
        {
            return Result.Failure<Guid>(userResult.Error);
        }

        var user = userResult.Value;

        var activationToken = _tokenGenerator.Generate(32);
        var activationTokenHash = _tokenHasher.Hash(activationToken);
        var activationTokenExpiresAt = DateTime.UtcNow.AddHours(24);

        user.SetActivationToken(activationTokenHash, activationTokenExpiresAt);

        await _userRepository.AddAsync(user, cancellationToken);

        var activationLink = $"https://app.idelivery.com/activate?token={activationToken}&email={Uri.EscapeDataString(command.Email)}";
        await _emailService.SendActivationEmailAsync(command.Email, activationLink, cancellationToken);

        return Result.Success(user.Id);
    }
}