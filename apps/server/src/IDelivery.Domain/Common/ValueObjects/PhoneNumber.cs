using System.Text.RegularExpressions;
using IDelivery.Domain.Common.Exceptions;

namespace IDelivery.Domain.Common.ValueObjects;

/// <summary>
/// Representa um número de telefone brasileiro.
/// Armazena internamente no formato internacional (55 + DDD + número).
/// Aceita formatos com e sem máscara.
/// </summary>
public sealed class PhoneNumber : ValueObject
{
    private static readonly Regex UnformattedPhoneRegex = new(
        @"^[0-9]{2}(?:9[0-9]{8}|[2-5][0-9]{7})$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex FormattedPhoneRegex = new(
        @"^\([0-9]{2}\) (?:9[0-9]{4}-[0-9]{4}|[2-5][0-9]{3}-[0-9]{4})$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly HashSet<string> ValidAreaCodes =
    [
        "11", "12", "13", "14", "15", "16", "17", "18", "19",
        "21", "22", "24",
        "27", "28",
        "31", "32", "33", "34", "35", "37", "38",
        "41", "42", "43", "44", "45", "46",
        "47", "48", "49",
        "51", "53", "54", "55",
        "61",
        "62", "63", "64",
        "65", "66", "67",
        "68", "69",
        "71", "73", "74", "75", "77", "79",
        "81", "82", "83", "84", "85", "86", "87", "88", "89",
        "91", "92", "93", "94", "95", "96", "97", "98", "99"
    ];

    private const string CountryCode = "55";

    /// <summary>
    /// Número armazenado somente com dígitos, incluindo código do país 55.
    /// Ex: 5543999999999 (celular) ou 554333333333 (fixo)
    /// </summary>
    public string Value { get; private set; } = null!;

    // Construtor para EF Core
    private PhoneNumber() { }

    private PhoneNumber(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            throw new DomainException("O número de telefone não pode ser vazio");

        var isFormatted = FormattedPhoneRegex.IsMatch(phone);
        var isUnformatted = UnformattedPhoneRegex.IsMatch(phone);

        if (!isFormatted && !isUnformatted)
            throw new DomainException(
                "O número de telefone deve estar no formato (43) 99999-9999 ou 43999999999 para celular, e (43) 3333-3333 ou 4333333333 para telefone fixo");

        var normalized = Regex.Replace(phone, @"[\s()-]", "");
        var areaCode = normalized[..2];

        if (!ValidAreaCodes.Contains(areaCode))
            throw new DomainException($"O DDD '{areaCode}' não é um DDD válido no Brasil");

        Value = $"{CountryCode}{normalized}";
    }

    public static PhoneNumber Create(string phone) => new(phone);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    /// <summary>
    /// Retorna o telefone no formato amigável: (43) 99999-9999 ou (43) 3333-3333
    /// </summary>
    public override string ToString()
    {
        var number = Value[CountryCode.Length..];
        var ddd = number[..2];
        var phone = number[2..];

        if (phone.Length == 9) // Celular
            return $"({ddd}) {phone[..5]}-{phone[5..]}";

        // Fixo
        return $"({ddd}) {phone[..4]}-{phone[4..]}";
    }
}