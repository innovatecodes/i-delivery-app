using IDelivery.Application.Abstractions.CQRS;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.Application.Abstractions.Security;
using IDelivery.SharedKernel.Common.Result;

namespace IDelivery.Application.Commands.Auth;

public sealed class ActivateAccountCommandHandler : ICommandHandler<ActivateAccountCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenHasher _tokenHasher;

    public ActivateAccountCommandHandler(
        IUserRepository userRepository,
        ITokenHasher tokenHasher)
    {
        _userRepository = userRepository;
        _tokenHasher = tokenHasher;
    }

    public async Task<Result> Handle(ActivateAccountCommand command, CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetByStatusAsync(IDelivery.Domain.Users.Enums.UserStatus.PendingActivation, cancellationToken);
        
        var user = users.FirstOrDefault(u => 
            u.ActivationTokenHash is not null && 
            u.ActivationTokenExpiresAt is not null &&
            u.ActivationTokenExpiresAt > DateTime.UtcNow &&
            _tokenHasher.Verify(command.Token, u.ActivationTokenHash));

        if (user is null)
        {
            return Result.Failure(new Error("Auth.InvalidActivationToken", "Token de ativação inválido ou expirado"));
        }

        var activationResult = user.Activate(command.Token);
        
        if (activationResult.IsFailure)
        {
            return Result.Failure(activationResult.Error);
        }

        return Result.Success();
    }
}