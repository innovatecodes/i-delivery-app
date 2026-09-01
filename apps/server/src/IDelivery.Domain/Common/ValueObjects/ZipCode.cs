using System.Text.RegularExpressions;
using IDelivery.Domain.Common.Exceptions;

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

    private ZipCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("O CEP não pode ser vazio");

        var cleaned = CleanZipCode(value);
        if (!ZipCodeRegex.IsMatch(cleaned))
            throw new DomainException("Formato de CEP brasileiro inválido (esperado: 00000-000)");

        DigitsOnly = cleaned.Replace("-", "");
        Value = FormatZipCode(DigitsOnly);
    }

    public static ZipCode Create(string value) => new(value);

    private static string CleanZipCode(string zipCode) => Regex.Replace(zipCode, @"[^\d]", "");

    private static string FormatZipCode(string digits) => $"{digits[..5]}-{digits[5..]}";

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return DigitsOnly;
    }

    public static implicit operator string(ZipCode zipCode) => zipCode.Value;
    public static explicit operator ZipCode(string value) => Create(value);

    public override string ToString() => Value;
}