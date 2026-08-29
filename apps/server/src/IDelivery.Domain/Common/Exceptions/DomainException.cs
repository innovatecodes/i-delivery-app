// Kernel Compartilhado - Exceptions de Domínio
// Exceptions base para validações de regras de negócio no domínio.
// Não dependem de infraestrutura, frameworks ou camadas superiores.

using System.Collections.ObjectModel;

namespace IDelivery.Domain.Common.Exceptions;

/// <summary>
/// Exception base para erros de domínio.
/// Carrega uma coleção de mensagens de erro imutável.
/// </summary>
public abstract class BaseException : Exception
{
    public IReadOnlyCollection<string> Errors { get; }

    protected BaseException(string message) : base(message)
    {
        Errors = new ReadOnlyCollection<string>(new List<string> { message });
    }

    protected BaseException(IEnumerable<string> errors) : base("Um ou mais erros de domínio ocorreram.")
    {
        ArgumentNullException.ThrowIfNull(errors, nameof(errors));
        Errors = new ReadOnlyCollection<string>(errors.ToList());
    }
}

/// <summary>
/// Exception lançada quando uma regra de negócio do domínio é violada.
/// Utilizada para validações de invariantes, value objects, aggregates, etc.
/// </summary>
public sealed class DomainException : BaseException
{
    public DomainException(string message) : base(message) { }

    public DomainException(IEnumerable<string> errors) : base(errors) { }

    /// <summary>
    /// Lança DomainException se a condição for verdadeira.
    /// </summary>
    public static void ThrowIf(bool hasError, string message)
    {
        if (hasError)
            throw new DomainException(message);
    }

    /// <summary>
    /// Lança DomainException se a condição for verdadeira.
    /// </summary>
    public static void ThrowIf(bool hasError, IEnumerable<string> errors)
    {
        if (hasError)
            throw new DomainException(errors);
    }
}