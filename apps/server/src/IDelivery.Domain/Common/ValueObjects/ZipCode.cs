using System.Text.RegularExpressions;
using IDelivery.SharedKernel.Common.Result;

namespace IDelivery.Domain.Common.ValueObjects;

/// <summary>
/// Representa um CEP (Código de Endereçamento Postal) brasileiro.
/// Formato: 00000-000
/// </summary>
public sealed class ZipCode : ValueObject
{
    private static readonly Regex ZipCodeRegex = new(
        @"^\d{5}-?\d{3}$",
        RegexOptions.Compiled);

    public string Value { get; private set; } = null!;
    public string DigitsOnly { get; private set; } = null!;

    // Construtor para EF Core
    private ZipCode() { }

    private ZipCode(string digitsOnly)
    {
        DigitsOnly = digitsOnly;
        Value = FormatZipCode(digitsOnly);
    }

    public static Result<ZipCode> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<ZipCode>(new Error("ZipCode.Empty", "O CEP não pode ser vazio"));

        var cleaned = CleanZipCode(value);
        if (!ZipCodeRegex.IsMatch(cleaned))
            return Result.Failure<ZipCode>(new Error("ZipCode.InvalidFormat", "Formato de CEP brasileiro inválido (esperado: 00000-000)"));

        var digitsOnly = cleaned.Replace("-", "");
        return Result.Success(new ZipCode(digitsOnly));
    }

    private static string CleanZipCode(string zipCode) => Regex.Replace(zipCode, @"[^\d]", "");

    private static string FormatZipCode(string digits) => $"{digits[..5]}-{digits[5..]}";

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return DigitsOnly;
    }

    public override string ToString() => Value;
}
