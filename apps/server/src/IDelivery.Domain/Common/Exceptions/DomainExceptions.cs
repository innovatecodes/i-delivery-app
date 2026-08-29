namespace IDelivery.Domain.Common.Exceptions;

/// <summary>
/// Exception lançada quando uma regra de negócio do domínio é violada.
/// Utilizada para validações de invariantes, value objects, aggregates, etc.
/// Não conhece HTTP - apenas representa violação de regra de negócio.
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