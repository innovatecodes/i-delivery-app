using System.Text.RegularExpressions;
using IDelivery.SharedKernel.Common.Result;

namespace IDelivery.Domain.Common.ValueObjects;

/// <summary>
/// Representa um endereço de e-mail válido.
/// Normaliza para lowercase e valida formato RFC básico.
/// </summary>
public sealed class Email : ValueObject
{
    private static readonly Regex EmailRegex = new(
        @"^[a-z0-9](?:[a-z0-9._+-]*[a-z0-9_+-])?@[a-z0-9](?:[a-z0-9-]*[a-z0-9])?(?:\.[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public string Value { get; private set; } = null!;

    // Construtor para EF Core
    private Email() { }

    private Email(string value)
    {
        Value = value.Trim().ToLowerInvariant();
    }

    public static Result<Email> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<Email>(new Error("Email.Empty", "O e-mail não pode ser vazio"));

        var trimmed = value.Trim().ToLowerInvariant();
        if (!EmailRegex.IsMatch(trimmed))
            return Result.Failure<Email>(new Error("Email.InvalidFormat", "Formato de e-mail inválido"));

        return Result.Success(new Email(trimmed));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
