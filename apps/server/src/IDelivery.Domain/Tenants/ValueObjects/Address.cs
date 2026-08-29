// Bounded Context: Tenants
// Value Objects específicos do contexto de Tenants.
// Compostos por Value Objects do Kernel Compartilhado.

using IDelivery.Domain.Common.ValueObjects;

namespace IDelivery.Domain.Tenants.ValueObjects;

/// <summary>
/// Endereço do tenant (restaurante/empresa).
/// Value Object composto - imutável, igualdade por componentes.
/// Utiliza ZipCode do kernel compartilhado para validação de CEP.
/// </summary>
public sealed class Address : ValueObject
{
    public string Street { get; }
    public string Number { get; }
    public string? Complement { get; }
    public string Neighborhood { get; }
    public string City { get; }
    public string State { get; }
    public ZipCode ZipCode { get; }
    public string? Reference { get; }

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

/// <summary>
/// Informações de contato do tenant.
/// Value Object composto - imutável, igualdade por componentes.
/// Utiliza Email e PhoneNumber do kernel compartilhado para validação.
/// </summary>
public sealed class ContactInfo : ValueObject
{
    public Email Email { get; }
    public PhoneNumber Phone { get; }
    public PhoneNumber? WhatsApp { get; }

    public ContactInfo(Email email, PhoneNumber phone, PhoneNumber? whatsApp = null)
    {
        Email = email;
        Phone = phone;
        WhatsApp = whatsApp;
    }

    /// <summary>
    /// Factory method que aceita strings e converte para Value Objects tipados.
    /// </summary>
    public static ContactInfo Create(string email, string phone, string? whatsApp = null)
    {
        return new ContactInfo(
            Email.Create(email),
            PhoneNumber.Create(phone),
            string.IsNullOrWhiteSpace(whatsApp) ? null : PhoneNumber.Create(whatsApp));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Email;
        yield return Phone;
        yield return WhatsApp;
    }
}