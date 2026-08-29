// Application Layer - Security Abstractions
// Abstração para geração de chaves de API.

namespace IDelivery.Application.Common.Security;

/// <summary>
/// Interface para geração de chaves de API.
/// Implementação criptograficamente segura fica na Infrastructure.
/// </summary>
public interface IApiKeyGenerator
{
    /// <summary>
    /// Gera uma chave de API aleatória (alphanumeric).
    /// </summary>
    /// <param name="length">Tamanho da chave (padrão 32).</param>
    /// <returns>Chave de API contendo letras e números.</returns>
    string Generate(int length = 32);
}