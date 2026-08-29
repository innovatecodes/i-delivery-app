namespace IDelivery.Application.Common.Exceptions;

/// <summary>
/// Exception lançada quando o acesso é negado (403 Forbidden).
/// </summary>
public sealed class ForbiddenException : ApplicationException
{
    public ForbiddenException(string message) : base(message) { }

    public ForbiddenException(IEnumerable<string> errors) : base(errors) { }
}