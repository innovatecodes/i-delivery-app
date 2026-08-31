using System.Text.RegularExpressions;
using IDelivery.Domain.Common.Exceptions;

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
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("O e-mail não pode ser vazio.");

        var trimmed = value.Trim().ToLowerInvariant();
        if (!EmailRegex.IsMatch(trimmed))
            throw new DomainException("Formato de e-mail inválido.");

        Value = trimmed;
    }

    public static Email Create(string value) => new(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public static implicit operator string(Email email) => email.Value;
    public static explicit operator Email(string value) => Create(value);

    public override string ToString() => Value;
}