// Application Layer - Security Abstractions
// Abstração para geração de senhas seguras.
// Permite que casos de uso gerem senhas sem conhecer detalhes de implementação.

namespace IDelivery.Application.Common.Security;

/// <summary>
/// Interface para geração de senhas seguras.
/// Implementação criptograficamente segura fica na Infrastructure.
/// </summary>
public interface IPasswordGenerator
{
    /// <summary>
    /// Gera uma senha aleatória criptograficamente segura.
    /// </summary>
    /// <param name="length">Tamanho da senha (mínimo 8, padrão 16).</param>
    /// <returns>Senha contendo maiúsculas, minúsculas, números e especiais.</returns>
    string Generate(int length = 16);
}