using System.Security.Cryptography;
using IDelivery.Application.Abstractions.Security;

namespace IDelivery.Infrastructure.Security;

public sealed class SecureTokenGenerator : ISecureTokenGenerator
{
    public string Generate(int length = 32)
    {
        // Garante que o tamanho solicitado seja válido antes de alocar o buffer
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(length);

        var bytes = new byte[length];

        // Preenche o buffer com bytes criptograficamente seguros
        RandomNumberGenerator.Fill(bytes);

        // Converte para Base64 e adapta o resultado para uso seguro em URLs
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }
}