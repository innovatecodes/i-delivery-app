namespace IDelivery.SharedKernel.Common.Exceptions;

/// <summary>
/// Exception base para erros de domínio e aplicação.
/// Carrega uma coleção de mensagens de erro imutável.
/// </summary>
public abstract class BaseException : Exception
{
    protected BaseException(string message) : base(message) { }

    protected BaseException(string message, Exception innerException) : base(message, innerException) { }
}