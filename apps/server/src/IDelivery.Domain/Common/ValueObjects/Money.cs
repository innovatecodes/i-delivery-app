using IDelivery.Domain.Common.Exceptions;

namespace IDelivery.Domain.Common.ValueObjects;

/// <summary>
/// Representa um valor monetário com moeda.
/// Imutável, com operações aritméticas seguras.
/// </summary>
public sealed class Money : ValueObject
{
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = null!;

    // Construtor para EF Core
    private Money() { }

    private Money(decimal amount, string currency = "BRL")
    {
        if (amount < 0)
            throw new DomainException("O valor não pode ser negativo");

        if (string.IsNullOrWhiteSpace(currency))
            throw new DomainException("A moeda não pode ser vazia");

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
            throw new DivideByZeroException("Não é possível dividir por zero");
        return Create(Amount / divisor, Currency);
    }

    private void EnsureSameCurrency(Money other)
    {
        if (Currency != other.Currency)
            throw new DomainException("Não é possível operar valores com moedas diferentes");
    }

    public static Money operator +(Money left, Money right) => left.Add(right);
    public static Money operator -(Money left, Money right) => left.Subtract(right);
    public static Money operator *(Money left, decimal right) => left.Multiply(right);
    public static Money operator /(Money left, decimal right) => left.Divide(right);

    public static implicit operator decimal(Money money) => money.Amount;
    public static explicit operator Money(decimal amount) => Create(amount);

    public override string ToString() => $"{Amount:N2} {Currency}";
}