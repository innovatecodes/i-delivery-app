using IDelivery.Application.Abstractions.CQRS;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.SharedKernel.Common.Result;

namespace IDelivery.Application.Commands.Auth;

public sealed class ForgotPasswordCommandHandler : ICommandHandler<ForgotPasswordCommand>
{
    private readonly IUserRepository _userRepository;

    public ForgotPasswordCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result> Handle(ForgotPasswordCommand command, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(command.Email, cancellationToken);

        if (user is null)
        {
            return Result.Success();
        }

        if (user.Status != IDelivery.Domain.Users.Enums.UserStatus.Active)
        {
            return Result.Success();
        }

        var result = user.RequestPasswordReset();

        if (result.IsFailure)
        {
            return result;
        }

        _userRepository.Update(user);

        return Result.Success();
    }
}
