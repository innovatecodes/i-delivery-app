namespace IDelivery.Application.Common.Exceptions;

/// <summary>
/// Exception lançada quando uma requisição é inválida (400 Bad Request).
/// </summary>
public sealed class BadRequestException : ApplicationException
{
    public BadRequestException(string message) : base(message) { }

    public BadRequestException(IEnumerable<string> errors) : base(errors) { }
}