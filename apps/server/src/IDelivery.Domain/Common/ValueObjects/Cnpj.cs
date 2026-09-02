using System.Text.RegularExpressions;
using IDelivery.SharedKernel.Common.Result;

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

    private Cnpj(string digitsOnly)
    {
        DigitsOnly = digitsOnly;
        Value = FormatCnpj(digitsOnly);
    }

    public static Result<Cnpj> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<Cnpj>(new Error("Cnpj.Empty", "O CNPJ não pode ser vazio"));

        var digitsOnly = CleanCnpj(value);

        if (!CnpjRegex.IsMatch(digitsOnly))
            return Result.Failure<Cnpj>(new Error("Cnpj.InvalidLength", "CNPJ deve conter exatamente 14 dígitos"));

        if (!IsValidCnpj(digitsOnly))
            return Result.Failure<Cnpj>(new Error("Cnpj.InvalidCheckDigits", "CNPJ inválido"));

        return Result.Success(new Cnpj(digitsOnly));
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

    public override string ToString() => Value;
}
