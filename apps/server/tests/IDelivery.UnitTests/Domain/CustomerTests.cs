using FluentAssertions;
using IDelivery.Domain.Common.ValueObjects;
using IDelivery.Domain.Customers.Entities;
using IDelivery.Domain.Customers.Events;
using Xunit;

namespace IDelivery.UnitTests.Domain;

public class CustomerTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Email _email = Email.Create("john@test.com").Value;

    private Customer CreateValidCustomer()
    {
        return Customer.Create(_tenantId, _userId, "John Doe", _email).Value;
    }

    private void AddValidAddress(Customer customer, bool isDefault = false)
    {
        customer.AddAddress(
            "Home", "Rua A", "100", null, "Centro",
            "São Paulo", "SP", "01000-000", null, isDefault);
    }

    #region Customer.Create

    [Fact]
    public void Create_WithValidData_ShouldCreateCustomer()
    {
        var result = Customer.Create(_tenantId, _userId, "John Doe", _email);

        result.IsSuccess.Should().BeTrue();
        var customer = result.Value;
        customer.Id.Should().NotBeEmpty();
        customer.TenantId.Should().Be(_tenantId);
        customer.UserId.Should().Be(_userId);
        customer.FullName.Should().Be("John Doe");
        customer.Email.Value.Should().Be("john@test.com");
        customer.IsActive.Should().BeTrue();
        customer.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_WithEmptyTenantId_ShouldFail()
    {
        var result = Customer.Create(Guid.Empty, _userId, "John Doe", _email);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Customer.TenantRequired");
    }

    [Fact]
    public void Create_WithEmptyUserId_ShouldFail()
    {
        var result = Customer.Create(_tenantId, Guid.Empty, "John Doe", _email);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Customer.UserRequired");
    }

    [Fact]
    public void Create_WithEmptyFullName_ShouldFail()
    {
        var result = Customer.Create(_tenantId, _userId, "", _email);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Customer.FullNameRequired");
    }

    [Fact]
    public void Create_WithFullNameTooLong_ShouldFail()
    {
        var longName = new string('A', 201);

        var result = Customer.Create(_tenantId, _userId, longName, _email);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Customer.FullNameTooLong");
    }

    [Fact]
    public void Create_WithNullEmail_ShouldFail()
    {
        var result = Customer.Create(_tenantId, _userId, "John Doe", null!);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Customer.EmailRequired");
    }

    [Fact]
    public void Create_ShouldRaiseCustomerCreatedDomainEvent()
    {
        var result = Customer.Create(_tenantId, _userId, "John Doe", _email);

        result.IsSuccess.Should().BeTrue();
        result.Value.DomainEvents.Should().ContainSingle(e => e is CustomerCreatedDomainEvent);
        var domainEvent = (CustomerCreatedDomainEvent)result.Value.DomainEvents.First();
        domainEvent.TenantId.Should().Be(_tenantId);
        domainEvent.UserId.Should().Be(_userId);
        domainEvent.Email.Should().Be("john@test.com");
    }

    #endregion

    #region Customer.UpdateProfile

    [Fact]
    public void UpdateProfile_WithValidData_ShouldUpdate()
    {
        var customer = CreateValidCustomer();

        var result = customer.UpdateProfile("Jane Doe", Email.Create("jane@test.com").Value, null, null);

        result.IsSuccess.Should().BeTrue();
        customer.FullName.Should().Be("Jane Doe");
        customer.Email.Value.Should().Be("jane@test.com");
        customer.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdateProfile_WithEmptyFullName_ShouldFail()
    {
        var customer = CreateValidCustomer();

        var result = customer.UpdateProfile("", _email, null, null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Customer.FullNameRequired");
    }

    [Fact]
    public void UpdateProfile_WithFullNameTooLong_ShouldFail()
    {
        var customer = CreateValidCustomer();
        var longName = new string('A', 201);

        var result = customer.UpdateProfile(longName, _email, null, null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Customer.FullNameTooLong");
    }

    [Fact]
    public void UpdateProfile_WithNullEmail_ShouldFail()
    {
        var customer = CreateValidCustomer();

        var result = customer.UpdateProfile("Jane Doe", null!, null, null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Customer.EmailRequired");
    }

    [Fact]
    public void UpdateProfile_ShouldRaiseCustomerUpdatedDomainEvent()
    {
        var customer = CreateValidCustomer();
        customer.ClearDomainEvents();

        customer.UpdateProfile("Jane Doe", _email, null, null);

        customer.DomainEvents.Should().ContainSingle(e => e is CustomerUpdatedDomainEvent);
        var domainEvent = (CustomerUpdatedDomainEvent)customer.DomainEvents.First();
        domainEvent.CustomerId.Should().Be(customer.Id);
        domainEvent.TenantId.Should().Be(_tenantId);
    }

    #endregion

    #region Customer.AddAddress

    [Fact]
    public void AddAddress_WithValidData_ShouldAddAddress()
    {
        var customer = CreateValidCustomer();

        var result = customer.AddAddress(
            "Home", "Rua A", "100", null, "Centro",
            "São Paulo", "SP", "01000-000", null);

        result.IsSuccess.Should().BeTrue();
        customer.Addresses.Should().HaveCount(1);
        customer.Addresses[0].Label.Should().Be("Home");
        customer.Addresses[0].IsDefault.Should().BeFalse();
    }

    [Fact]
    public void AddAddress_AsDefault_ShouldUnsetDefaultOnOtherAddresses()
    {
        var customer = CreateValidCustomer();
        AddValidAddress(customer, isDefault: true);
        customer.Addresses[0].IsDefault.Should().BeTrue();

        customer.AddAddress(
            "Work", "Rua B", "200", null, "Centro",
            "São Paulo", "SP", "02000-000", null, isDefault: true);

        customer.Addresses.Should().HaveCount(2);
        customer.Addresses[0].IsDefault.Should().BeFalse();
        customer.Addresses[1].IsDefault.Should().BeTrue();
    }

    [Fact]
    public void AddAddress_WithEmptyLabel_ShouldFail()
    {
        var customer = CreateValidCustomer();

        var result = customer.AddAddress(
            "", "Rua A", "100", null, "Centro",
            "São Paulo", "SP", "01000-000", null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Customer.AddressLabelRequired");
    }

    [Fact]
    public void AddAddress_WithLabelTooLong_ShouldFail()
    {
        var customer = CreateValidCustomer();
        var longLabel = new string('A', 51);

        var result = customer.AddAddress(
            longLabel, "Rua A", "100", null, "Centro",
            "São Paulo", "SP", "01000-000", null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Customer.AddressLabelTooLong");
    }

    [Fact]
    public void AddAddress_ShouldRaiseCustomerAddressAddedDomainEvent()
    {
        var customer = CreateValidCustomer();
        customer.ClearDomainEvents();

        customer.AddAddress(
            "Home", "Rua A", "100", null, "Centro",
            "São Paulo", "SP", "01000-000", null);

        customer.DomainEvents.Should().ContainSingle(e => e is CustomerAddressAddedDomainEvent);
        var domainEvent = (CustomerAddressAddedDomainEvent)customer.DomainEvents.First();
        domainEvent.CustomerId.Should().Be(customer.Id);
        domainEvent.TenantId.Should().Be(_tenantId);
    }

    #endregion

    #region Customer.RemoveAddress

    [Fact]
    public void RemoveAddress_ExistingAddress_ShouldRemove()
    {
        var customer = CreateValidCustomer();
        AddValidAddress(customer);
        var addressId = customer.Addresses[0].Id;

        var result = customer.RemoveAddress(addressId);

        result.IsSuccess.Should().BeTrue();
        customer.Addresses.Should().BeEmpty();
    }

    [Fact]
    public void RemoveAddress_NonExistentAddress_ShouldFail()
    {
        var customer = CreateValidCustomer();

        var result = customer.RemoveAddress(Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Customer.AddressNotFound");
    }

    [Fact]
    public void RemoveAddress_ShouldRaiseCustomerAddressRemovedDomainEvent()
    {
        var customer = CreateValidCustomer();
        AddValidAddress(customer);
        var addressId = customer.Addresses[0].Id;
        customer.ClearDomainEvents();

        customer.RemoveAddress(addressId);

        customer.DomainEvents.Should().ContainSingle(e => e is CustomerAddressRemovedDomainEvent);
        var domainEvent = (CustomerAddressRemovedDomainEvent)customer.DomainEvents.First();
        domainEvent.AddressId.Should().Be(addressId);
    }

    #endregion

    #region Customer.SetDefaultAddress

    [Fact]
    public void SetDefaultAddress_ExistingAddress_ShouldSetDefault()
    {
        var customer = CreateValidCustomer();
        AddValidAddress(customer);
        AddValidAddress(customer);
        var secondAddressId = customer.Addresses[1].Id;

        var result = customer.SetDefaultAddress(secondAddressId);

        result.IsSuccess.Should().BeTrue();
        customer.Addresses[0].IsDefault.Should().BeFalse();
        customer.Addresses[1].IsDefault.Should().BeTrue();
    }

    [Fact]
    public void SetDefaultAddress_NonExistentAddress_ShouldFail()
    {
        var customer = CreateValidCustomer();

        var result = customer.SetDefaultAddress(Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Customer.AddressNotFound");
    }

    #endregion

    #region Customer.Activate / Deactivate

    [Fact]
    public void Activate_WhenInactive_ShouldActivate()
    {
        var customer = CreateValidCustomer();
        customer.Deactivate();

        var result = customer.Activate();

        result.IsSuccess.Should().BeTrue();
        customer.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Activate_WhenAlreadyActive_ShouldFail()
    {
        var customer = CreateValidCustomer();

        var result = customer.Activate();

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Customer.AlreadyActive");
    }

    [Fact]
    public void Deactivate_WhenActive_ShouldDeactivate()
    {
        var customer = CreateValidCustomer();

        var result = customer.Deactivate();

        result.IsSuccess.Should().BeTrue();
        customer.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_ShouldFail()
    {
        var customer = CreateValidCustomer();
        customer.Deactivate();

        var result = customer.Deactivate();

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Customer.AlreadyInactive");
    }

    #endregion

    #region CustomerAddress.Create

    [Fact]
    public void AddressCreate_WithValidData_ShouldCreateAddress()
    {
        var customerId = Guid.NewGuid();

        var result = CustomerAddress.Create(
            customerId, "Home", "Rua A", "100", null, "Centro",
            "São Paulo", "SP", "01000-000", null);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().NotBeEmpty();
        result.Value.CustomerId.Should().Be(customerId);
        result.Value.Label.Should().Be("Home");
        result.Value.IsDefault.Should().BeFalse();
    }

    [Fact]
    public void AddressCreate_WithEmptyCustomerId_ShouldFail()
    {
        var result = CustomerAddress.Create(
            Guid.Empty, "Home", "Rua A", "100", null, "Centro",
            "São Paulo", "SP", "01000-000", null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CustomerAddress.CustomerRequired");
    }

    [Fact]
    public void AddressCreate_WithEmptyLabel_ShouldFail()
    {
        var result = CustomerAddress.Create(
            Guid.NewGuid(), "", "Rua A", "100", null, "Centro",
            "São Paulo", "SP", "01000-000", null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CustomerAddress.LabelRequired");
    }

    [Fact]
    public void AddressCreate_WithLabelTooLong_ShouldFail()
    {
        var longLabel = new string('A', 51);

        var result = CustomerAddress.Create(
            Guid.NewGuid(), longLabel, "Rua A", "100", null, "Centro",
            "São Paulo", "SP", "01000-000", null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CustomerAddress.LabelTooLong");
    }

    [Fact]
    public void AddressCreate_WithEmptyStreet_ShouldFail()
    {
        var result = CustomerAddress.Create(
            Guid.NewGuid(), "Home", "", "100", null, "Centro",
            "São Paulo", "SP", "01000-000", null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CustomerAddress.StreetRequired");
    }

    [Fact]
    public void AddressCreate_WithStreetTooLong_ShouldFail()
    {
        var longStreet = new string('A', 201);

        var result = CustomerAddress.Create(
            Guid.NewGuid(), "Home", longStreet, "100", null, "Centro",
            "São Paulo", "SP", "01000-000", null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CustomerAddress.StreetTooLong");
    }

    [Fact]
    public void AddressCreate_WithEmptyNumber_ShouldFail()
    {
        var result = CustomerAddress.Create(
            Guid.NewGuid(), "Home", "Rua A", "", null, "Centro",
            "São Paulo", "SP", "01000-000", null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CustomerAddress.NumberRequired");
    }

    [Fact]
    public void AddressCreate_WithNumberTooLong_ShouldFail()
    {
        var longNumber = new string('1', 21);

        var result = CustomerAddress.Create(
            Guid.NewGuid(), "Home", "Rua A", longNumber, null, "Centro",
            "São Paulo", "SP", "01000-000", null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CustomerAddress.NumberTooLong");
    }

    [Fact]
    public void AddressCreate_WithEmptyNeighborhood_ShouldFail()
    {
        var result = CustomerAddress.Create(
            Guid.NewGuid(), "Home", "Rua A", "100", null, "",
            "São Paulo", "SP", "01000-000", null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CustomerAddress.NeighborhoodRequired");
    }

    [Fact]
    public void AddressCreate_WithNeighborhoodTooLong_ShouldFail()
    {
        var longNeighborhood = new string('A', 101);

        var result = CustomerAddress.Create(
            Guid.NewGuid(), "Home", "Rua A", "100", null, longNeighborhood,
            "São Paulo", "SP", "01000-000", null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CustomerAddress.NeighborhoodTooLong");
    }

    [Fact]
    public void AddressCreate_WithEmptyCity_ShouldFail()
    {
        var result = CustomerAddress.Create(
            Guid.NewGuid(), "Home", "Rua A", "100", null, "Centro",
            "", "SP", "01000-000", null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CustomerAddress.CityRequired");
    }

    [Fact]
    public void AddressCreate_WithCityTooLong_ShouldFail()
    {
        var longCity = new string('A', 101);

        var result = CustomerAddress.Create(
            Guid.NewGuid(), "Home", "Rua A", "100", null, "Centro",
            longCity, "SP", "01000-000", null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CustomerAddress.CityTooLong");
    }

    [Fact]
    public void AddressCreate_WithEmptyState_ShouldFail()
    {
        var result = CustomerAddress.Create(
            Guid.NewGuid(), "Home", "Rua A", "100", null, "Centro",
            "São Paulo", "", "01000-000", null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CustomerAddress.StateRequired");
    }

    [Fact]
    public void AddressCreate_WithStateTooLong_ShouldFail()
    {
        var result = CustomerAddress.Create(
            Guid.NewGuid(), "Home", "Rua A", "100", null, "Centro",
            "São Paulo", "SPO", "01000-000", null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CustomerAddress.StateTooLong");
    }

    [Fact]
    public void AddressCreate_WithEmptyZipCode_ShouldFail()
    {
        var result = CustomerAddress.Create(
            Guid.NewGuid(), "Home", "Rua A", "100", null, "Centro",
            "São Paulo", "SP", "", null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CustomerAddress.ZipCodeRequired");
    }

    [Fact]
    public void AddressCreate_WithZipCodeTooLong_ShouldFail()
    {
        var longZipCode = new string('0', 11);

        var result = CustomerAddress.Create(
            Guid.NewGuid(), "Home", "Rua A", "100", null, "Centro",
            "São Paulo", "SP", longZipCode, null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CustomerAddress.ZipCodeTooLong");
    }

    #endregion

    #region CustomerAddress.UpdateDetails

    [Fact]
    public void AddressUpdateDetails_WithValidData_ShouldUpdate()
    {
        var address = CustomerAddress.Create(
            Guid.NewGuid(), "Home", "Rua A", "100", null, "Centro",
            "São Paulo", "SP", "01000-000", null).Value;

        var result = address.UpdateDetails(
            "Work", "Rua B", "200", "Apto 1", "Jardins",
            "São Paulo", "SP", "02000-000", "Ref");

        result.IsSuccess.Should().BeTrue();
        address.Label.Should().Be("Work");
        address.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void AddressUpdateDetails_WithEmptyLabel_ShouldFail()
    {
        var address = CustomerAddress.Create(
            Guid.NewGuid(), "Home", "Rua A", "100", null, "Centro",
            "São Paulo", "SP", "01000-000", null).Value;

        var result = address.UpdateDetails(
            "", "Rua B", "200", null, "Jardins",
            "São Paulo", "SP", "02000-000", null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CustomerAddress.LabelRequired");
    }

    [Fact]
    public void AddressUpdateDetails_WithEmptyStreet_ShouldFail()
    {
        var address = CustomerAddress.Create(
            Guid.NewGuid(), "Home", "Rua A", "100", null, "Centro",
            "São Paulo", "SP", "01000-000", null).Value;

        var result = address.UpdateDetails(
            "Work", "", "200", null, "Jardins",
            "São Paulo", "SP", "02000-000", null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CustomerAddress.StreetRequired");
    }

    [Fact]
    public void AddressUpdateDetails_WithEmptyNumber_ShouldFail()
    {
        var address = CustomerAddress.Create(
            Guid.NewGuid(), "Home", "Rua A", "100", null, "Centro",
            "São Paulo", "SP", "01000-000", null).Value;

        var result = address.UpdateDetails(
            "Work", "Rua B", "", null, "Jardins",
            "São Paulo", "SP", "02000-000", null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CustomerAddress.NumberRequired");
    }

    [Fact]
    public void AddressUpdateDetails_WithEmptyNeighborhood_ShouldFail()
    {
        var address = CustomerAddress.Create(
            Guid.NewGuid(), "Home", "Rua A", "100", null, "Centro",
            "São Paulo", "SP", "01000-000", null).Value;

        var result = address.UpdateDetails(
            "Work", "Rua B", "200", null, "",
            "São Paulo", "SP", "02000-000", null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CustomerAddress.NeighborhoodRequired");
    }

    [Fact]
    public void AddressUpdateDetails_WithEmptyCity_ShouldFail()
    {
        var address = CustomerAddress.Create(
            Guid.NewGuid(), "Home", "Rua A", "100", null, "Centro",
            "São Paulo", "SP", "01000-000", null).Value;

        var result = address.UpdateDetails(
            "Work", "Rua B", "200", null, "Jardins",
            "", "SP", "02000-000", null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CustomerAddress.CityRequired");
    }

    [Fact]
    public void AddressUpdateDetails_WithEmptyState_ShouldFail()
    {
        var address = CustomerAddress.Create(
            Guid.NewGuid(), "Home", "Rua A", "100", null, "Centro",
            "São Paulo", "SP", "01000-000", null).Value;

        var result = address.UpdateDetails(
            "Work", "Rua B", "200", null, "Jardins",
            "São Paulo", "", "02000-000", null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CustomerAddress.StateRequired");
    }

    [Fact]
    public void AddressUpdateDetails_WithEmptyZipCode_ShouldFail()
    {
        var address = CustomerAddress.Create(
            Guid.NewGuid(), "Home", "Rua A", "100", null, "Centro",
            "São Paulo", "SP", "01000-000", null).Value;

        var result = address.UpdateDetails(
            "Work", "Rua B", "200", null, "Jardins",
            "São Paulo", "SP", "", null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CustomerAddress.ZipCodeRequired");
    }

    #endregion

    #region CustomerAddress.SetAsDefault / UnsetDefault

    [Fact]
    public void SetAsDefault_ShouldMarkAsDefault()
    {
        var address = CustomerAddress.Create(
            Guid.NewGuid(), "Home", "Rua A", "100", null, "Centro",
            "São Paulo", "SP", "01000-000", null, isDefault: false).Value;

        address.SetAsDefault();

        address.IsDefault.Should().BeTrue();
        address.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void UnsetDefault_ShouldUnmarkDefault()
    {
        var address = CustomerAddress.Create(
            Guid.NewGuid(), "Home", "Rua A", "100", null, "Centro",
            "São Paulo", "SP", "01000-000", null, isDefault: true).Value;

        address.UnsetDefault();

        address.IsDefault.Should().BeFalse();
        address.UpdatedAt.Should().NotBeNull();
    }

    #endregion
}
