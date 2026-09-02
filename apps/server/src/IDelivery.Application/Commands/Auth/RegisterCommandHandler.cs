using IDelivery.Application.Abstractions.CQRS;
using IDelivery.Application.Abstractions.Messaging;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.Application.Abstractions.Security;
using IDelivery.Domain.Common.ValueObjects;
using IDelivery.Domain.Roles;
using IDelivery.Domain.Users.Entities;
using IDelivery.SharedKernel.Common.Result;
using Microsoft.VisualBasic.FileIO;
using System.Runtime;

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
        IEmailService emailService
        )
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

        var emailResult = Email.Create(command.Email);
        if (emailResult.IsFailure)
            return Result.Failure<Guid>(emailResult.Error);

        var email = emailResult.Value;

        PhoneNumber? phoneNumber = null;
        if (!string.IsNullOrWhiteSpace(command.PhoneNumber))
        {
            var phoneResult = PhoneNumber.Create(command.PhoneNumber);
            if (phoneResult.IsFailure)
                return Result.Failure<Guid>(phoneResult.Error);
            phoneNumber = phoneResult.Value;
        }

        var passwordHash = _passwordHasher.Hash(command.Password);

        var userResult = User.Create(
            email,
            passwordHash,
            command.FullName,
            Role.Customer,
            null,
            phoneNumber);

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

        var activationPath = $"/activate?token={Uri.EscapeDataString(activationToken)}&email={Uri.EscapeDataString(command.Email)}";
        
        await _emailService.SendActivationEmailAsync(command.Email, activationPath, cancellationToken);

        return Result.Success(user.Id);
    }
}
