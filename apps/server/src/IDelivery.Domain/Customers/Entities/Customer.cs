using IDelivery.SharedKernel.Common.Result;
using IDelivery.Domain.Common.ValueObjects;
using IDelivery.Domain.Common.Entities;
using IDelivery.Domain.Customers.Events;

namespace IDelivery.Domain.Customers.Entities;

/// <summary>
/// Aggregate Root do Cliente.
/// Representa o perfil do cliente dentro de um tenant.
/// Um cliente pode ter múltiplos endereços.
/// </summary>
public sealed class Customer : AggregateRoot
{
    public Guid TenantId { get; private set; }
    public Guid UserId { get; private set; }
    public string FullName { get; private set; } = null!;
    public Email Email { get; private set; } = null!;
    public PhoneNumber? PhoneNumber { get; private set; }
    public string? Notes { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private readonly List<CustomerAddress> _addresses = [];
    public IReadOnlyList<CustomerAddress> Addresses => _addresses.AsReadOnly();

    private Customer() { }

    private Customer(
        Guid id,
        Guid tenantId,
        Guid userId,
        string fullName,
        Email email,
        PhoneNumber? phoneNumber,
        string? notes) : base(id)
    {
        TenantId = tenantId;
        UserId = userId;
        FullName = fullName;
        Email = email;
        PhoneNumber = phoneNumber;
        Notes = notes;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;

        AddDomainEvent(new CustomerCreatedDomainEvent(id, tenantId, userId, email.Value));
    }

    /// <summary>
    /// Factory method para criar um novo cliente.
    /// </summary>
    public static Result<Customer> Create(
        Guid tenantId,
        Guid userId,
        string fullName,
        Email email,
        PhoneNumber? phoneNumber = null,
        string? notes = null)
    {
        if (tenantId == Guid.Empty)
            return Result.Failure<Customer>(new Error("Customer.TenantRequired", "Tenant é obrigatório"));

        if (userId == Guid.Empty)
            return Result.Failure<Customer>(new Error("Customer.UserRequired", "Usuário é obrigatório"));

        if (string.IsNullOrWhiteSpace(fullName))
            return Result.Failure<Customer>(new Error("Customer.FullNameRequired", "Nome completo é obrigatório"));

        if (fullName.Length > 200)
            return Result.Failure<Customer>(new Error("Customer.FullNameTooLong", "Nome completo deve ter no máximo 200 caracteres"));

        if (email is null)
            return Result.Failure<Customer>(new Error("Customer.EmailRequired", "Email é obrigatório"));

        var customer = new Customer(
            Guid.NewGuid(),
            tenantId,
            userId,
            fullName.Trim(),
            email,
            phoneNumber,
            notes?.Trim());

        return Result.Success(customer);
    }

    /// <summary>
    /// Atualiza os dados do perfil do cliente.
    /// </summary>
    public Result UpdateProfile(
        string fullName,
        Email email,
        PhoneNumber? phoneNumber,
        string? notes)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return Result.Failure(new Error("Customer.FullNameRequired", "Nome completo é obrigatório"));

        if (fullName.Length > 200)
            return Result.Failure(new Error("Customer.FullNameTooLong", "Nome completo deve ter no máximo 200 caracteres"));

        if (email is null)
            return Result.Failure(new Error("Customer.EmailRequired", "Email é obrigatório"));

        FullName = fullName.Trim();
        Email = email;
        PhoneNumber = phoneNumber;
        Notes = notes?.Trim();
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new CustomerUpdatedDomainEvent(Id, TenantId, UserId));

        return Result.Success();
    }

    /// <summary>
    /// Adiciona um endereço ao cliente.
    /// </summary>
    public Result AddAddress(
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
        if (string.IsNullOrWhiteSpace(label))
            return Result.Failure(new Error("Customer.AddressLabelRequired", "Rótulo do endereço é obrigatório"));

        if (label.Length > 50)
            return Result.Failure(new Error("Customer.AddressLabelTooLong", "Rótulo do endereço deve ter no máximo 50 caracteres"));

        if (isDefault)
        {
            foreach (var addr in _addresses)
            {
                addr.UnsetDefault();
            }
        }

        var addressResult = CustomerAddress.Create(
            Id, label, street, number, complement, neighborhood, city, state, zipCode, reference, isDefault);

        if (addressResult.IsFailure)
            return Result.Failure(addressResult.Error);

        _addresses.Add(addressResult.Value);
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new CustomerAddressAddedDomainEvent(Id, TenantId, addressResult.Value.Id));

        return Result.Success();
    }

    /// <summary>
    /// Remove um endereço do cliente.
    /// </summary>
    public Result RemoveAddress(Guid addressId)
    {
        var address = _addresses.FirstOrDefault(a => a.Id == addressId);
        if (address is null)
            return Result.Failure(new Error("Customer.AddressNotFound", "Endereço não encontrado"));

        _addresses.Remove(address);
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new CustomerAddressRemovedDomainEvent(Id, TenantId, addressId));

        return Result.Success();
    }

    /// <summary>
    /// Define um endereço como padrão.
    /// </summary>
    public Result SetDefaultAddress(Guid addressId)
    {
        var address = _addresses.FirstOrDefault(a => a.Id == addressId);
        if (address is null)
            return Result.Failure(new Error("Customer.AddressNotFound", "Endereço não encontrado"));

        foreach (var addr in _addresses)
        {
            addr.UnsetDefault();
        }

        address.SetAsDefault();
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    /// <summary>
    /// Ativa o cliente.
    /// </summary>
    public Result Activate()
    {
        if (IsActive)
            return Result.Failure(new Error("Customer.AlreadyActive", "Cliente já está ativo"));

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    /// <summary>
    /// Desativa o cliente.
    /// </summary>
    public Result Deactivate()
    {
        if (!IsActive)
            return Result.Failure(new Error("Customer.AlreadyInactive", "Cliente já está inativo"));

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }
}