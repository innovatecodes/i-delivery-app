namespace IDelivery.Application.Common.Exceptions;

/// <summary>
/// Exception lançada quando um recurso não é encontrado (404 Not Found).
/// </summary>
public sealed class NotFoundException : ApplicationException
{
    public NotFoundException(string message) : base(message) { }

    public NotFoundException(IEnumerable<string> errors) : base(errors) { }
}