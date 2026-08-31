using IDelivery.Domain.Common.ValueObjects;

namespace IDelivery.Domain.Tenants.ValueObjects;

/// <summary>
/// Endereço do tenant (restaurante/empresa).
/// Value Object composto - imutável, igualdade por componentes.
/// Utiliza ZipCode do kernel compartilhado para validação de CEP.
/// </summary>
public sealed class Address : ValueObject
{
    public string Street { get; private set; } = null!;
    public string Number { get; private set; } = null!;
    public string? Complement { get; private set; }
    public string Neighborhood { get; private set; } = null!;
    public string City { get; private set; } = null!;
    public string State { get; private set; } = null!;
    public ZipCode ZipCode { get; private set; } = null!;
    public string? Reference { get; private set; }

    // Construtor para EF Core (apenas para materialização)
    private Address() { }

    public Address(
        string street,
        string number,
        string? complement,
        string neighborhood,
        string city,
        string state,
        ZipCode zipCode,
        string? reference = null)
    {
        Street = street;
        Number = number;
        Complement = complement;
        Neighborhood = neighborhood;
        City = city;
        State = state;
        ZipCode = zipCode;
        Reference = reference;
    }

    /// <summary>
    /// Factory method que aceita string de CEP e converte para ZipCode.
    /// </summary>
    public static Address Create(
        string street,
        string number,
        string? complement,
        string neighborhood,
        string city,
        string state,
        string zipCode,
        string? reference = null)
    {
        return new Address(
            street, number, complement, neighborhood, city, state,
            ZipCode.Create(zipCode), reference);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Street;
        yield return Number;
        yield return Complement;
        yield return Neighborhood;
        yield return City;
        yield return State;
        yield return ZipCode;
        yield return Reference;
    }
}

