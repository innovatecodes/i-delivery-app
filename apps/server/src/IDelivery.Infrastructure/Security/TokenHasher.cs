using BCrypt.Net;
using IDelivery.Application.Abstractions.Security;

namespace IDelivery.Infrastructure.Security;

public sealed class TokenHasher : ITokenHasher
{
    public string Hash(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        return BCrypt.Net.BCrypt.HashPassword(token);
    }

    public bool Verify(string token, string tokenHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenHash);

        return BCrypt.Net.BCrypt.Verify(token, tokenHash);
    }
}