using IDelivery.Application.Abstractions.Authentication;
using IDelivery.Application.Abstractions.CQRS;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.Application.Abstractions.Security;
using IDelivery.Application.Common.Models;
using IDelivery.Domain.Users.Enums;
using IDelivery.SharedKernel.Common.Result;

namespace IDelivery.Application.Commands.Auth;

public sealed class RefreshTokenCommandHandler : ICommandHandler<RefreshTokenCommand, AuthResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ITokenHasher _tokenHasher;
    private readonly ISecureTokenGenerator _tokenGenerator;

    public RefreshTokenCommandHandler(
        IUserRepository userRepository,
        IJwtTokenService jwtTokenService,
        ITokenHasher tokenHasher,
        ISecureTokenGenerator tokenGenerator)
    {
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
        _tokenHasher = tokenHasher;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<Result<AuthResult>> Handle(RefreshTokenCommand command, CancellationToken cancellationToken = default)
    {
        var principal = _jwtTokenService.GetPrincipalFromExpiredToken(command.RefreshToken);
        
        if (principal is null)
        {
            return Result.Failure<AuthResult>(new Error("Auth.InvalidRefreshToken", "Refresh token inválido"));
        }

        var userIdClaim = principal.FindFirst("sub")?.Value;
        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Result.Failure<AuthResult>(new Error("Auth.InvalidRefreshToken", "Refresh token inválido"));
        }

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        
        if (user is null || user.Status != UserStatus.Active)
        {
            return Result.Failure<AuthResult>(new Error("Auth.InvalidRefreshToken", "Refresh token inválido"));
        }

        if (user.ResetPasswordTokenHash is null || user.ResetPasswordTokenExpiresAt is null)
        {
            return Result.Failure<AuthResult>(new Error("Auth.InvalidRefreshToken", "Refresh token inválido"));
        }

        if (!_tokenHasher.Verify(command.RefreshToken, user.ResetPasswordTokenHash) || user.ResetPasswordTokenExpiresAt < DateTime.UtcNow)
        {
            return Result.Failure<AuthResult>(new Error("Auth.InvalidRefreshToken", "Refresh token inválido ou expirado"));
        }

        var newRefreshToken = _tokenGenerator.Generate(64);
        var newRefreshTokenHash = _tokenHasher.Hash(newRefreshToken);
        var newRefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);

        user.SetResetPasswordToken(newRefreshTokenHash, newRefreshTokenExpiresAt);

        var roles = new[] { user.Role.ToString() };
        var accessToken = _jwtTokenService.GenerateAccessToken(user.Id, user.TenantId, roles);
        var accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(15);

        return Result.Success(new AuthResult(
            accessToken,
            newRefreshToken,
            accessTokenExpiresAt,
            user.Id,
            user.Email.Value,
            user.FullName,
            roles,
            user.TenantId));
    }
}