namespace IDelivery.Application.Common.Exceptions;

/// <summary>
/// Exception lançada quando há conflito (ex: duplicidade) (409 Conflict).
/// </summary>
public sealed class ConflictException : ApplicationException
{
    public ConflictException(string message) : base(message) { }

    public ConflictException(IEnumerable<string> errors) : base(errors) { }
}