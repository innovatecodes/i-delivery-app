namespace IDelivery.Application.Common.Exceptions;

/// <summary>
/// Exception lançada quando uma validação de entrada falha (400 Bad Request).
/// </summary>
public sealed class ValidationException : ApplicationException
{
    public ValidationException(string message) : base(message) { }

    public ValidationException(IEnumerable<string> errors) : base(errors) { }
}