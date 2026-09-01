using IDelivery.SharedKernel.Common.Result;
using IDelivery.Domain.Common.Entities;

namespace IDelivery.Domain.Customers.Entities;

/// <summary>
/// Entidade que representa um endereço do cliente.
/// Não é Aggregate Root — é gerenciada pelo Customer.
/// </summary>
public sealed class CustomerAddress : Entity
{
    public Guid CustomerId { get; private set; }
    public string Label { get; private set; } = null!;
    public string Street { get; private set; } = null!;
    public string Number { get; private set; } = null!;
    public string? Complement { get; private set; }
    public string Neighborhood { get; private set; } = null!;
    public string City { get; private set; } = null!;
    public string State { get; private set; } = null!;
    public string ZipCode { get; private set; } = null!;
    public string? Reference { get; private set; }
    public bool IsDefault { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private CustomerAddress() { }

    private CustomerAddress(
        Guid id,
        Guid customerId,
        string label,
        string street,
        string number,
        string? complement,
        string neighborhood,
        string city,
        string state,
        string zipCode,
        string? reference,
        bool isDefault) : base(id)
    {
        CustomerId = customerId;
        Label = label;
        Street = street;
        Number = number;
        Complement = complement;
        Neighborhood = neighborhood;
        City = city;
        State = state;
        ZipCode = zipCode;
        Reference = reference;
        IsDefault = isDefault;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Factory method para criar um novo endereço do cliente.
    /// </summary>
    public static Result<CustomerAddress> Create(
        Guid customerId,
        string label,
        string street,
        string number,
        string? complement,
        string neighborhood,
        string city,
        string state,
        string zipCode,
        string? reference,
        bool isDefault = false)
    {
        if (customerId == Guid.Empty)
            return Result.Failure<CustomerAddress>(new Error("CustomerAddress.CustomerRequired", "Cliente é obrigatório"));

        if (string.IsNullOrWhiteSpace(label))
            return Result.Failure<CustomerAddress>(new Error("CustomerAddress.LabelRequired", "Rótulo é obrigatório"));

        if (label.Length > 50)
            return Result.Failure<CustomerAddress>(new Error("CustomerAddress.LabelTooLong", "Rótulo deve ter no máximo 50 caracteres"));

        if (string.IsNullOrWhiteSpace(street))
            return Result.Failure<CustomerAddress>(new Error("CustomerAddress.StreetRequired", "Rua é obrigatória"));

        if (street.Length > 200)
            return Result.Failure<CustomerAddress>(new Error("CustomerAddress.StreetTooLong", "Rua deve ter no máximo 200 caracteres"));

        if (string.IsNullOrWhiteSpace(number))
            return Result.Failure<CustomerAddress>(new Error("CustomerAddress.NumberRequired", "Número é obrigatório"));

        if (number.Length > 20)
            return Result.Failure<CustomerAddress>(new Error("CustomerAddress.NumberTooLong", "Número deve ter no máximo 20 caracteres"));

        if (string.IsNullOrWhiteSpace(neighborhood))
            return Result.Failure<CustomerAddress>(new Error("CustomerAddress.NeighborhoodRequired", "Bairro é obrigatório"));

        if (neighborhood.Length > 100)
            return Result.Failure<CustomerAddress>(new Error("CustomerAddress.NeighborhoodTooLong", "Bairro deve ter no máximo 100 caracteres"));

        if (string.IsNullOrWhiteSpace(city))
            return Result.Failure<CustomerAddress>(new Error("CustomerAddress.CityRequired", "Cidade é obrigatória"));

        if (city.Length > 100)
            return Result.Failure<CustomerAddress>(new Error("CustomerAddress.CityTooLong", "Cidade deve ter no máximo 100 caracteres"));

        if (string.IsNullOrWhiteSpace(state))
            return Result.Failure<CustomerAddress>(new Error("CustomerAddress.StateRequired", "Estado é obrigatório"));

        if (state.Length > 2)
            return Result.Failure<CustomerAddress>(new Error("CustomerAddress.StateTooLong", "Estado deve ter no máximo 2 caracteres"));

        if (string.IsNullOrWhiteSpace(zipCode))
            return Result.Failure<CustomerAddress>(new Error("CustomerAddress.ZipCodeRequired", "CEP é obrigatório"));

        if (zipCode.Length > 10)
            return Result.Failure<CustomerAddress>(new Error("CustomerAddress.ZipCodeTooLong", "CEP deve ter no máximo 10 caracteres"));

        var address = new CustomerAddress(
            Guid.NewGuid(),
            customerId,
            label.Trim(),
            street.Trim(),
            number.Trim(),
            complement?.Trim(),
            neighborhood.Trim(),
            city.Trim(),
            state.Trim().ToUpperInvariant(),
            zipCode.Trim(),
            reference?.Trim(),
            isDefault);

        return Result.Success(address);
    }

    /// <summary>
    /// Atualiza os dados do endereço.
    /// </summary>
    public Result UpdateDetails(
        string label,
        string street,
        string number,
        string? complement,
        string neighborhood,
        string city,
        string state,
        string zipCode,
        string? reference)
    {
        if (string.IsNullOrWhiteSpace(label))
            return Result.Failure(new Error("CustomerAddress.LabelRequired", "Rótulo é obrigatório"));

        if (string.IsNullOrWhiteSpace(street))
            return Result.Failure(new Error("CustomerAddress.StreetRequired", "Rua é obrigatória"));

        if (string.IsNullOrWhiteSpace(number))
            return Result.Failure(new Error("CustomerAddress.NumberRequired", "Número é obrigatório"));

        if (string.IsNullOrWhiteSpace(neighborhood))
            return Result.Failure(new Error("CustomerAddress.NeighborhoodRequired", "Bairro é obrigatório"));

        if (string.IsNullOrWhiteSpace(city))
            return Result.Failure(new Error("CustomerAddress.CityRequired", "Cidade é obrigatória"));

        if (string.IsNullOrWhiteSpace(state))
            return Result.Failure(new Error("CustomerAddress.StateRequired", "Estado é obrigatório"));

        if (string.IsNullOrWhiteSpace(zipCode))
            return Result.Failure(new Error("CustomerAddress.ZipCodeRequired", "CEP é obrigatório"));

        Label = label.Trim();
        Street = street.Trim();
        Number = number.Trim();
        Complement = complement?.Trim();
        Neighborhood = neighborhood.Trim();
        City = city.Trim();
        State = state.Trim().ToUpperInvariant();
        ZipCode = zipCode.Trim();
        Reference = reference?.Trim();
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    /// <summary>
    /// Define este endereço como padrão.
    /// </summary>
    public void SetAsDefault()
    {
        IsDefault = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Remove a marca de padrão deste endereço.
    /// </summary>
    public void UnsetDefault()
    {
        IsDefault = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
