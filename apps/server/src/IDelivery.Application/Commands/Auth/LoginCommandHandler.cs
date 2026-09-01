using IDelivery.Application.Abstractions.CQRS;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.Application.Abstractions.Security;
using IDelivery.Application.Common.Models;
using IDelivery.Domain.Users.Enums;
using IDelivery.SharedKernel.Common.Result;

namespace IDelivery.Application.Commands.Auth;

public sealed class LoginCommandHandler : ICommandHandler<LoginCommand, AuthResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ISecureTokenGenerator _tokenGenerator;
    private readonly ITokenHasher _tokenHasher;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        ISecureTokenGenerator tokenGenerator,
        ITokenHasher tokenHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _tokenGenerator = tokenGenerator;
        _tokenHasher = tokenHasher;
    }

    public async Task<Result<AuthResult>> Handle(LoginCommand command, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(command.Email, cancellationToken);

        if (user is null)
        {
            return Result.Failure<AuthResult>(new Error("Auth.InvalidCredentials", "Credenciais inválidas"));
        }

        if (!_passwordHasher.Verify(command.Password, user.PasswordHash!))
        {
            return Result.Failure<AuthResult>(new Error("Auth.InvalidCredentials", "Credenciais inválidas"));
        }

        if (user.Status != UserStatus.Active)
        {
            return Result.Failure<AuthResult>(new Error("Auth.AccountNotActive", "Conta não está ativa. Verifique seu e-mail para ativação"));
        }

        var refreshToken = _tokenGenerator.Generate(64);
        var refreshTokenHash = _tokenHasher.Hash(refreshToken);
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);

        user.SetResetPasswordToken(refreshTokenHash, refreshTokenExpiresAt);
        user.RecordLogin();

        var roles = new[] { user.Role.ToString() };
        var accessToken = _jwtTokenService.GenerateAccessToken(user.Id, user.TenantId, roles);
        var accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(15);

        return Result.Success(new AuthResult(
            accessToken,
            refreshToken,
            accessTokenExpiresAt,
            user.Id,
            user.Email.Value,
            user.FullName,
            roles,
            user.TenantId));
    }
}