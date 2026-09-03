using IDelivery.Application.Abstractions.CQRS;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.Application.Abstractions.Security;
using IDelivery.Domain.Common.ValueObjects;
using IDelivery.Domain.Roles;
using IDelivery.Domain.Users.Entities;
using IDelivery.SharedKernel.Common.Result;

namespace IDelivery.Application.Commands.Auth;

public sealed class RegisterCommandHandler : ICommandHandler<RegisterCommand, Guid>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
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

        await _userRepository.AddAsync(user, cancellationToken);

        return Result.Success(user.Id);
    }
}
