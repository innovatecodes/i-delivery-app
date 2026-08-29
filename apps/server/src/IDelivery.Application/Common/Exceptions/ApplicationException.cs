using System.Collections.ObjectModel;

namespace IDelivery.Application.Common.Exceptions;

/// <summary>
/// Exception base para erros da camada de Aplicação.
/// Carrega uma coleção de mensagens de erro imutável.
/// </summary>
public abstract class ApplicationException : Exception
{
    public IReadOnlyCollection<string> Errors { get; }

    protected ApplicationException(string message) : base(message)
    {
        Errors = new ReadOnlyCollection<string>(new List<string> { message });
    }

    protected ApplicationException(IEnumerable<string> errors) : base("Um ou mais erros de aplicação ocorreram.")
    {
        ArgumentNullException.ThrowIfNull(errors, nameof(errors));
        Errors = new ReadOnlyCollection<string>(errors.ToList());
    }
}