namespace IDelivery.Application.Common.Exceptions;

/// <summary>
/// Exception lançada quando não há autenticação ou credenciais inválidas (401 Unauthorized).
/// </summary>
public sealed class UnauthorizedException : ApplicationException
{
    public UnauthorizedException(string message) : base(message) { }

    public UnauthorizedException(IEnumerable<string> errors) : base(errors) { }
}