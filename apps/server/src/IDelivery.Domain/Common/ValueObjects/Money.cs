using IDelivery.Domain.Common.Exceptions;
using IDelivery.SharedKernel.Common.Result;

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

    private Money(decimal amount, string currency)
    {
        Amount = Math.Round(amount, 2);
        Currency = currency.Trim().ToUpperInvariant();
    }

    public static Result<Money> Create(decimal amount, string currency = "BRL")
    {
        if (amount < 0)
            return Result.Failure<Money>(new Error("Money.NegativeAmount", "O valor não pode ser negativo"));

        if (string.IsNullOrWhiteSpace(currency))
            return Result.Failure<Money>(new Error("Money.EmptyCurrency", "A moeda não pode ser vazia"));

        return Result.Success(new Money(amount, currency));
    }

    public static Money Zero(string currency = "BRL") => Create(0, currency).Value;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public Money Add(Money other)
    {
        ArgumentNullException.ThrowIfNull(other);
        EnsureSameCurrency(other);
        return Create(Amount + other.Amount, Currency).Value;
    }

    public Money Subtract(Money other)
    {
        ArgumentNullException.ThrowIfNull(other);
        EnsureSameCurrency(other);
        var result = Amount - other.Amount;
        return Create(result < 0 ? 0 : result, Currency).Value;
    }

    public Money Multiply(decimal multiplier) => Create(Amount * multiplier, Currency).Value;

    public Money Divide(decimal divisor)
    {
        if (divisor == 0)
            throw new DivideByZeroException("Não é possível dividir por zero");
        return Create(Amount / divisor, Currency).Value;
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
    public static explicit operator Money(decimal amount) => Create(amount).Value;

    public override string ToString() => $"{Amount:N2} {Currency}";
}
