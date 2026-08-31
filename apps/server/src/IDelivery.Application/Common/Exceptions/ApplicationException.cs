using IDelivery.SharedKernel.Common.Exceptions;

namespace IDelivery.Application.Common.Exceptions;

/// <summary>
/// Exceção base para erros da camada de Aplicação.
/// Representa falhas relacionadas à execução de casos de uso,
/// validações e regras específicas da aplicação.
/// Não conhece HTTP nem detalhes de infraestrutura.
/// </summary>
public abstract class ApplicationException : BaseException
{
    public IReadOnlyCollection<string> Errors { get; }

    protected ApplicationException(string message)
        : base(message)
    {
        Errors = new[] { message };
    }

    protected ApplicationException(IEnumerable<string> errors)
        : base("Um ou mais erros de aplicação ocorreram.")
    {
        ArgumentNullException.ThrowIfNull(errors);

        Errors = errors.ToList();
    }
}

