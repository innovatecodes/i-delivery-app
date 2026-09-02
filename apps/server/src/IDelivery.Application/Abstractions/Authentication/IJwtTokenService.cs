using System.Security.Claims;

namespace IDelivery.Application.Abstractions.Authentication;

public interface IJwtTokenService
{
    string GenerateAccessToken(Guid userId, Guid? tenantId, string[] roles);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    bool ValidateToken(string token);
}