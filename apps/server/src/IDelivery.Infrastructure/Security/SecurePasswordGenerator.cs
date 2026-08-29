// Infrastructure Layer - Security Implementation
// Implementação criptograficamente segura para geração de senhas e chaves.
// Usa RandomNumberGenerator (CSPRNG) para segurança.

using System.Security.Cryptography;
using IDelivery.Application.Common.Security;

namespace IDelivery.Infrastructure.Security;

/// <summary>
/// Gerador de senhas seguras usando RandomNumberGenerator (CSPRNG).
/// Garante entropia criptográfica adequada para senhas e chaves.
/// </summary>
public sealed class SecurePasswordGenerator : IPasswordGenerator, IApiKeyGenerator
{
    private static readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();

    private const string Uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string Lowercase = "abcdefghijklmnopqrstuvwxyz";
    private const string Digits = "0123456789";
    private const string Special = "!@#$%^&*";
    private const string Alphanumeric = Uppercase + Lowercase + Digits;

    /// <inheritdoc />
    public string Generate(int length = 16)
    {
        return GenerateSecurePassword(length);
    }

    /// <inheritdoc />
    string IApiKeyGenerator.Generate(int length = 32)
    {
        return GenerateApiKey(length);
    }

    /// <summary>
    /// Gera uma senha aleatória criptograficamente segura.
    /// Garante pelo menos 1 maiúscula, 1 minúscula, 1 dígito, 1 especial.
    /// </summary>
    /// <param name="length">Tamanho da senha (mínimo 8, padrão 16).</param>
    /// <returns>Senha segura embaralhada.</returns>
    /// <exception cref="ArgumentException">Se length < 8.</exception>
    public string GenerateSecurePassword(int length = 16)
    {
        if (length < 8)
            throw new ArgumentException("Password length must be at least 8", nameof(length));

        var chars = new char[length];
        var allChars = Uppercase + Lowercase + Digits + Special;

        // Garante pelo menos 1 de cada categoria obrigatória
        chars[0] = GetRandomChar(Uppercase);
        chars[1] = GetRandomChar(Lowercase);
        chars[2] = GetRandomChar(Digits);
        chars[3] = GetRandomChar(Special);

        // Preenche o restante
        for (int i = 4; i < length; i++)
        {
            chars[i] = GetRandomChar(allChars);
        }

        // Embaralha (Fisher-Yates)
        for (int i = chars.Length - 1; i > 0; i--)
        {
            int j = GetRandomInt(i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }

        return new string(chars);
    }

    /// <summary>
    /// Gera uma chave de API alfanumérica criptograficamente segura.
    /// </summary>
    /// <param name="length">Tamanho da chave (padrão 32).</param>
    /// <returns>Chave de API (A-Z, a-z, 0-9).</returns>
    public string GenerateApiKey(int length = 32)
    {
        var result = new char[length];

        for (int i = 0; i < length; i++)
        {
            result[i] = GetRandomChar(Alphanumeric);
        }

        return new string(result);
    }

    private char GetRandomChar(string chars)
    {
        var index = GetRandomInt(chars.Length);
        return chars[index];
    }

    private int GetRandomInt(int maxExclusive)
    {
        var buffer = new byte[4];
        _rng.GetBytes(buffer);
        var value = BitConverter.ToUInt32(buffer, 0);
        return (int)(value % (uint)maxExclusive);
    }
}