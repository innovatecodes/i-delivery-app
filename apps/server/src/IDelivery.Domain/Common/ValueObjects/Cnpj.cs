// Kernel Compartilhado - Value Objects de Domínio
// Value Objects reutilizáveis em múltiplos Bounded Contexts.
// Contêm validação de formato, normalização e igualdade estrutural.

using System.Text.RegularExpressions;
using IDelivery.Domain.Common.Exceptions;

namespace IDelivery.Domain.Common.ValueObjects;

/// <summary>
/// Representa um CNPJ brasileiro válido.
/// Valida dígitos verificadores e formata automaticamente.
/// </summary>
public sealed class Cnpj : ValueObject
{
    private static readonly Regex CnpjRegex = new(
        @"^\d{14}$",
        RegexOptions.Compiled);

    public string Value { get; private set; } = null!;
    public string DigitsOnly { get; private set; } = null!;

    // Construtor para EF Core
    private Cnpj() { }

    private Cnpj(string value)
    {
        DigitsOnly = CleanCnpj(value);

        if (!CnpjRegex.IsMatch(DigitsOnly))
            throw new DomainException("CNPJ deve conter exatamente 14 dígitos.");

        if (!IsValidCnpj(DigitsOnly))
            throw new DomainException("CNPJ inválido.");

        Value = FormatCnpj(DigitsOnly);
    }

    public static Cnpj Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("O CNPJ não pode ser vazio.");

        return new Cnpj(value);
    }

    private static string CleanCnpj(string cnpj) => Regex.Replace(cnpj, @"[^\d]", "");

    private static string FormatCnpj(string digits) =>
        $"{digits[..2]}.{digits[2..5]}.{digits[5..8]}/{digits[8..12]}-{digits[12..]}";

    private static bool IsValidCnpj(string digits)
    {
        if (digits.All(digit => digit == digits[0]))
            return false;

        var firstDigit = CalculateCheckDigit(digits[..12]);
        var secondDigit = CalculateCheckDigit(digits[..12] + firstDigit);

        return digits[12] == firstDigit && digits[13] == secondDigit;
    }

    private static char CalculateCheckDigit(string digits)
    {
        var weights = digits.Length == 12
            ? new[] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 }
            : new[] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

        var sum = 0;
        for (var i = 0; i < digits.Length; i++)
            sum += (digits[i] - '0') * weights[i];

        var remainder = sum % 11;
        var digit = remainder < 2 ? 0 : 11 - remainder;

        return (char)('0' + digit);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return DigitsOnly;
    }

    public static implicit operator string?(Cnpj? cnpj) => cnpj?.Value;
    public static explicit operator Cnpj(string value) => Create(value);

    public override string ToString() => Value;
}