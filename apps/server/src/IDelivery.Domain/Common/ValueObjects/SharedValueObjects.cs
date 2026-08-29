// Kernel Compartilhado - Value Objects de Domínio
// Value Objects reutilizáveis em múltiplos Bounded Contexts.
// Contêm validação de formato, normalização e igualdade estrutural.

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

    public string Value { get; }

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
    public string Value { get; }

    private PhoneNumber(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            throw new DomainException("O número de telefone não pode ser vazio.");

        var isFormatted = FormattedPhoneRegex.IsMatch(phone);
        var isUnformatted = UnformattedPhoneRegex.IsMatch(phone);

        if (!isFormatted && !isUnformatted)
            throw new DomainException(
                "O número de telefone deve estar no formato (43) 99999-9999 ou 43999999999 para celular, e (43) 3333-3333 ou 4333333333 para telefone fixo.");

        var normalized = Regex.Replace(phone, @"[\s()-]", "");
        var areaCode = normalized[..2];

        if (!ValidAreaCodes.Contains(areaCode))
            throw new DomainException($"O DDD '{areaCode}' não é um DDD válido no Brasil.");

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

/// <summary>
/// Representa um valor monetário com moeda.
/// Imutável, com operações aritméticas seguras.
/// </summary>
public sealed class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency = "BRL")
    {
        if (amount < 0)
            throw new DomainException("O valor não pode ser negativo.");

        if (string.IsNullOrWhiteSpace(currency))
            throw new DomainException("A moeda não pode ser vazia.");

        Amount = Math.Round(amount, 2);
        Currency = currency.Trim().ToUpperInvariant();
    }

    public static Money Create(decimal amount, string currency = "BRL") => new(amount, currency);
    public static Money Zero(string currency = "BRL") => Create(0, currency);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public Money Add(Money other)
    {
        ArgumentNullException.ThrowIfNull(other);
        EnsureSameCurrency(other);
        return Create(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        ArgumentNullException.ThrowIfNull(other);
        EnsureSameCurrency(other);
        var result = Amount - other.Amount;
        return Create(result < 0 ? 0 : result, Currency);
    }

    public Money Multiply(decimal multiplier) => Create(Amount * multiplier, Currency);

    public Money Divide(decimal divisor)
    {
        if (divisor == 0)
            throw new DivideByZeroException("Não é possível dividir por zero.");
        return Create(Amount / divisor, Currency);
    }

    private void EnsureSameCurrency(Money other)
    {
        if (Currency != other.Currency)
            throw new DomainException("Não é possível operar valores com moedas diferentes.");
    }

    public static Money operator +(Money left, Money right) => left.Add(right);
    public static Money operator -(Money left, Money right) => left.Subtract(right);
    public static Money operator *(Money left, decimal right) => left.Multiply(right);
    public static Money operator /(Money left, decimal right) => left.Divide(right);

    public static implicit operator decimal(Money money) => money.Amount;
    public static explicit operator Money(decimal amount) => Create(amount);

    public override string ToString() => $"{Amount:N2} {Currency}";
}

/// <summary>
/// Representa um CEP (Código de Endereçamento Postal) brasileiro.
/// Formato: 00000-000
/// </summary>
public sealed class ZipCode : ValueObject
{
    private static readonly Regex ZipCodeRegex = new(
        @"^\d{5}-?\d{3}$",
        RegexOptions.Compiled);

    public string Value { get; }
    public string DigitsOnly { get; }

    private ZipCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("O CEP não pode ser vazio.");

        var cleaned = CleanZipCode(value);
        if (!ZipCodeRegex.IsMatch(cleaned))
            throw new DomainException("Formato de CEP brasileiro inválido (esperado: 00000-000).");

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

/// <summary>
/// Representa coordenadas geográficas (latitude, longitude).
/// Valida ranges válidos e fornece cálculo de distância (Haversine).
/// </summary>
public sealed class Coordinates : ValueObject
{
    public decimal Latitude { get; }
    public decimal Longitude { get; }

    private Coordinates(decimal latitude, decimal longitude)
    {
        if (latitude < -90 || latitude > 90)
            throw new DomainException("A latitude deve estar entre -90 e 90");
        if (longitude < -180 || longitude > 180)
            throw new DomainException("A longitude deve estar entre -180 e 180");

        Latitude = Math.Round(latitude, 6);
        Longitude = Math.Round(longitude, 6);
    }

    public static Coordinates Create(decimal latitude, decimal longitude) => new(latitude, longitude);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Latitude;
        yield return Longitude;
    }

    /// <summary>
    /// Calcula distância em km até outra coordenada (fórmula Haversine).
    /// </summary>
    public double DistanceTo(Coordinates other)
    {
        const double earthRadiusKm = 6371;

        var lat1 = (double)Latitude * Math.PI / 180;
        var lat2 = (double)other.Latitude * Math.PI / 180;
        var deltaLat = ((double)other.Latitude - (double)Latitude) * Math.PI / 180;
        var deltaLon = ((double)other.Longitude - (double)Longitude) * Math.PI / 180;

        var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                Math.Cos(lat1) * Math.Cos(lat2) *
                Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return earthRadiusKm * c;
    }

    public static implicit operator (decimal Lat, decimal Lng)(Coordinates coords) => (coords.Latitude, coords.Longitude);
    public static implicit operator Coordinates((decimal Lat, decimal Lng) tuple) => Create(tuple.Lat, tuple.Lng);

    public override string ToString() => $"{Latitude}, {Longitude}";
}

/// <summary>
/// Representa um CNPJ brasileiro válido.
/// Valida dígitos verificadores e formata automaticamente.
/// </summary>
public sealed class Cnpj : ValueObject
{
    private static readonly Regex CnpjRegex = new(
        @"^\d{14}$",
        RegexOptions.Compiled);

    public string Value { get; }
    public string DigitsOnly { get; }

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