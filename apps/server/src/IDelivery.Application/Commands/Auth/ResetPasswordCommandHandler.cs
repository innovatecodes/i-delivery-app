using IDelivery.Application.Abstractions.CQRS;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.Application.Abstractions.Security;
using IDelivery.SharedKernel.Common.Result;

namespace IDelivery.Application.Commands.Auth;

public sealed class ResetPasswordCommandHandler : ICommandHandler<ResetPasswordCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenHasher _tokenHasher;

    public ResetPasswordCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenHasher tokenHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenHasher = tokenHasher;
    }

    public async Task<Result> Handle(ResetPasswordCommand command, CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetByStatusAsync(IDelivery.Domain.Users.Enums.UserStatus.Active, cancellationToken);
        
        var user = users.FirstOrDefault(u => 
            u.ResetPasswordTokenHash is not null && 
            u.ResetPasswordTokenExpiresAt is not null &&
            u.ResetPasswordTokenExpiresAt > DateTime.UtcNow &&
            _tokenHasher.Verify(command.Token, u.ResetPasswordTokenHash));

        if (user is null)
        {
            return Result.Failure(new Error("Auth.InvalidResetToken", "Token de redefinição inválido ou expirado"));
        }

        var newPasswordHash = _passwordHasher.Hash(command.NewPassword);
        var result = user.ResetPassword(newPasswordHash);

        return result;
    }
}