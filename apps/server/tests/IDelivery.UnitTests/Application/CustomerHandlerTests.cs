using FluentAssertions;
using IDelivery.Application.Abstractions.Authentication;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.Application.Commands.Customers;
using IDelivery.Domain.Common.ValueObjects;
using IDelivery.Domain.Customers.Entities;
using Moq;
using System;
using System.Threading;
using Xunit;

namespace IDelivery.UnitTests.Application;

public class CreateCustomerCommandHandlerTests
{
    private readonly Mock<ICustomerRepository> _mockCustomerRepository;
    private readonly Mock<ITenantContext> _mockTenantContext;
    private readonly Guid _tenantId = Guid.NewGuid();

    public CreateCustomerCommandHandlerTests()
    {
        _mockCustomerRepository = new Mock<ICustomerRepository>();
        _mockTenantContext = new Mock<ITenantContext>();
        _mockTenantContext.Setup(x => x.TenantId).Returns(_tenantId);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateCustomer()
    {
        var command = new CreateCustomerCommand(
            Guid.NewGuid(),
            "João da Silva",
            "joao@test.com",
            "(11) 99999-9999",
            "Cliente VIP");

        var handler = new CreateCustomerCommandHandler(
            _mockCustomerRepository.Object,
            _mockTenantContext.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        _mockCustomerRepository.Verify(x => x.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithoutTenant_ShouldFail()
    {
        _mockTenantContext.Setup(x => x.TenantId).Returns((Guid?)null);

        var command = new CreateCustomerCommand(
            Guid.NewGuid(),
            "João da Silva",
            "joao@test.com",
            null,
            null);

        var handler = new CreateCustomerCommandHandler(
            _mockCustomerRepository.Object,
            _mockTenantContext.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Customer.TenantRequired");
    }

    [Fact]
    public async Task Handle_WithExistingEmail_ShouldFail()
    {
        _mockCustomerRepository.Setup(x => x.ExistsByEmailAsync(
            _tenantId, "existing@test.com", It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new CreateCustomerCommand(
            Guid.NewGuid(),
            "João da Silva",
            "existing@test.com",
            null,
            null);

        var handler = new CreateCustomerCommandHandler(
            _mockCustomerRepository.Object,
            _mockTenantContext.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Customer.EmailAlreadyExists");
    }
}

public class UpdateCustomerCommandHandlerTests
{
    private readonly Mock<ICustomerRepository> _mockCustomerRepository;
    private readonly Mock<ITenantContext> _mockTenantContext;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _customerId = Guid.NewGuid();

    public UpdateCustomerCommandHandlerTests()
    {
        _mockCustomerRepository = new Mock<ICustomerRepository>();
        _mockTenantContext = new Mock<ITenantContext>();
        _mockTenantContext.Setup(x => x.TenantId).Returns(_tenantId);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateCustomer()
    {
        var email = Email.Create("joao@test.com").Value;
        var customerResult = Customer.Create(_tenantId, Guid.NewGuid(), "João da Silva", email);
        Assert.True(customerResult.IsSuccess);
        var customer = customerResult.Value;

        _mockCustomerRepository.Setup(x => x.GetByIdAsync(_customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        var command = new UpdateCustomerCommand(
            _customerId,
            "João Atualizado",
            "joao_updated@test.com",
            "(11) 98888-8888",
            "Observação atualizada");

        var handler = new UpdateCustomerCommandHandler(
            _mockCustomerRepository.Object,
            _mockTenantContext.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _mockCustomerRepository.Verify(x => x.UpdateAsync(customer, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithoutTenant_ShouldFail()
    {
        _mockTenantContext.Setup(x => x.TenantId).Returns((Guid?)null);

        var command = new UpdateCustomerCommand(
            _customerId,
            "João da Silva",
            "joao@test.com",
            null,
            null);

        var handler = new UpdateCustomerCommandHandler(
            _mockCustomerRepository.Object,
            _mockTenantContext.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Customer.TenantRequired");
    }

    [Fact]
    public async Task Handle_WithNonExistentCustomer_ShouldFail()
    {
        _mockCustomerRepository.Setup(x => x.GetByIdAsync(_customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        var command = new UpdateCustomerCommand(
            _customerId,
            "João da Silva",
            "joao@test.com",
            null,
            null);

        var handler = new UpdateCustomerCommandHandler(
            _mockCustomerRepository.Object,
            _mockTenantContext.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Customer.NotFound");
    }

    [Fact]
    public async Task Handle_WithTenantMismatch_ShouldFail()
    {
        var otherTenantId = Guid.NewGuid();
        var email = Email.Create("joao@test.com").Value;
        var customerResult = Customer.Create(otherTenantId, Guid.NewGuid(), "João da Silva", email);
        Assert.True(customerResult.IsSuccess);
        var customer = customerResult.Value;

        _mockCustomerRepository.Setup(x => x.GetByIdAsync(_customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        var command = new UpdateCustomerCommand(
            _customerId,
            "João da Silva",
            "joao@test.com",
            null,
            null);

        var handler = new UpdateCustomerCommandHandler(
            _mockCustomerRepository.Object,
            _mockTenantContext.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Customer.AccessDenied");
    }
}

public class DeleteCustomerCommandHandlerTests
{
    private readonly Mock<ICustomerRepository> _mockCustomerRepository;
    private readonly Mock<ITenantContext> _mockTenantContext;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _customerId = Guid.NewGuid();

    public DeleteCustomerCommandHandlerTests()
    {
        _mockCustomerRepository = new Mock<ICustomerRepository>();
        _mockTenantContext = new Mock<ITenantContext>();
        _mockTenantContext.Setup(x => x.TenantId).Returns(_tenantId);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldDeactivateCustomer()
    {
        var email = Email.Create("joao@test.com").Value;
        var customerResult = Customer.Create(_tenantId, Guid.NewGuid(), "João da Silva", email);
        Assert.True(customerResult.IsSuccess);
        var customer = customerResult.Value;

        _mockCustomerRepository.Setup(x => x.GetByIdAsync(_customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        var command = new DeleteCustomerCommand(_customerId);

        var handler = new DeleteCustomerCommandHandler(
            _mockCustomerRepository.Object,
            _mockTenantContext.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        customer.IsActive.Should().BeFalse();
        _mockCustomerRepository.Verify(x => x.UpdateAsync(customer, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithoutTenant_ShouldFail()
    {
        _mockTenantContext.Setup(x => x.TenantId).Returns((Guid?)null);

        var command = new DeleteCustomerCommand(_customerId);

        var handler = new DeleteCustomerCommandHandler(
            _mockCustomerRepository.Object,
            _mockTenantContext.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Customer.TenantRequired");
    }

    [Fact]
    public async Task Handle_WithNonExistentCustomer_ShouldFail()
    {
        _mockCustomerRepository.Setup(x => x.GetByIdAsync(_customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        var command = new DeleteCustomerCommand(_customerId);

        var handler = new DeleteCustomerCommandHandler(
            _mockCustomerRepository.Object,
            _mockTenantContext.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Customer.NotFound");
    }

    [Fact]
    public async Task Handle_WithTenantMismatch_ShouldFail()
    {
        var otherTenantId = Guid.NewGuid();
        var email = Email.Create("joao@test.com").Value;
        var customerResult = Customer.Create(otherTenantId, Guid.NewGuid(), "João da Silva", email);
        Assert.True(customerResult.IsSuccess);
        var customer = customerResult.Value;

        _mockCustomerRepository.Setup(x => x.GetByIdAsync(_customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        var command = new DeleteCustomerCommand(_customerId);

        var handler = new DeleteCustomerCommandHandler(
            _mockCustomerRepository.Object,
            _mockTenantContext.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Customer.AccessDenied");
    }

    [Fact]
    public async Task Handle_WhenAlreadyInactive_ShouldFail()
    {
        var email = Email.Create("joao@test.com").Value;
        var customerResult = Customer.Create(_tenantId, Guid.NewGuid(), "João da Silva", email);
        Assert.True(customerResult.IsSuccess);
        var customer = customerResult.Value;
        customer.Deactivate();

        _mockCustomerRepository.Setup(x => x.GetByIdAsync(_customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        var command = new DeleteCustomerCommand(_customerId);

        var handler = new DeleteCustomerCommandHandler(
            _mockCustomerRepository.Object,
            _mockTenantContext.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Customer.AlreadyInactive");
    }
}

public class AddCustomerAddressCommandHandlerTests
{
    private readonly Mock<ICustomerRepository> _mockCustomerRepository;
    private readonly Mock<ITenantContext> _mockTenantContext;
    private readonly Mock<ICurrentUser> _mockCurrentUser;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    public AddCustomerAddressCommandHandlerTests()
    {
        _mockCustomerRepository = new Mock<ICustomerRepository>();
        _mockTenantContext = new Mock<ITenantContext>();
        _mockCurrentUser = new Mock<ICurrentUser>();
        _mockTenantContext.Setup(x => x.TenantId).Returns(_tenantId);
        _mockCurrentUser.Setup(x => x.UserId).Returns(_userId);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldAddAddress()
    {
        var email = Email.Create("joao@test.com").Value;
        var customerResult = Customer.Create(_tenantId, _userId, "João da Silva", email);
        Assert.True(customerResult.IsSuccess);
        var customer = customerResult.Value;

        _mockCustomerRepository.Setup(x => x.GetByUserIdAsync(_tenantId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        var command = new AddCustomerAddressCommand(
            "Casa",
            "Rua das Flores",
            "123",
            "Apto 1",
            "Centro",
            "São Paulo",
            "SP",
            "01001-000",
            "Próximo ao parque",
            false);

        var handler = new AddCustomerAddressCommandHandler(
            _mockCustomerRepository.Object,
            _mockTenantContext.Object,
            _mockCurrentUser.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        customer.Addresses.Should().HaveCount(1);
        _mockCustomerRepository.Verify(x => x.UpdateAsync(customer, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithoutTenant_ShouldFail()
    {
        _mockTenantContext.Setup(x => x.TenantId).Returns((Guid?)null);

        var command = new AddCustomerAddressCommand(
            "Casa",
            "Rua das Flores",
            "123",
            null,
            "Centro",
            "São Paulo",
            "SP",
            "01001-000",
            null,
            false);

        var handler = new AddCustomerAddressCommandHandler(
            _mockCustomerRepository.Object,
            _mockTenantContext.Object,
            _mockCurrentUser.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Customer.TenantRequired");
    }

    [Fact]
    public async Task Handle_WithoutUser_ShouldFail()
    {
        _mockCurrentUser.Setup(x => x.UserId).Returns((Guid?)null);

        var command = new AddCustomerAddressCommand(
            "Casa",
            "Rua das Flores",
            "123",
            null,
            "Centro",
            "São Paulo",
            "SP",
            "01001-000",
            null,
            false);

        var handler = new AddCustomerAddressCommandHandler(
            _mockCustomerRepository.Object,
            _mockTenantContext.Object,
            _mockCurrentUser.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Customer.UserRequired");
    }

    [Fact]
    public async Task Handle_WithNonExistentCustomer_ShouldFail()
    {
        _mockCustomerRepository.Setup(x => x.GetByUserIdAsync(_tenantId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        var command = new AddCustomerAddressCommand(
            "Casa",
            "Rua das Flores",
            "123",
            null,
            "Centro",
            "São Paulo",
            "SP",
            "01001-000",
            null,
            false);

        var handler = new AddCustomerAddressCommandHandler(
            _mockCustomerRepository.Object,
            _mockTenantContext.Object,
            _mockCurrentUser.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Customer.NotFound");
    }
}

public class RemoveCustomerAddressCommandHandlerTests
{
    private readonly Mock<ICustomerRepository> _mockCustomerRepository;
    private readonly Mock<ITenantContext> _mockTenantContext;
    private readonly Mock<ICurrentUser> _mockCurrentUser;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    public RemoveCustomerAddressCommandHandlerTests()
    {
        _mockCustomerRepository = new Mock<ICustomerRepository>();
        _mockTenantContext = new Mock<ITenantContext>();
        _mockCurrentUser = new Mock<ICurrentUser>();
        _mockTenantContext.Setup(x => x.TenantId).Returns(_tenantId);
        _mockCurrentUser.Setup(x => x.UserId).Returns(_userId);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldRemoveAddress()
    {
        var email = Email.Create("joao@test.com").Value;
        var customerResult = Customer.Create(_tenantId, _userId, "João da Silva", email);
        Assert.True(customerResult.IsSuccess);
        var customer = customerResult.Value;

        var addResult = customer.AddAddress(
            "Casa", "Rua das Flores", "123", null, "Centro",
            "São Paulo", "SP", "01001-000", null, false);
        Assert.True(addResult.IsSuccess);
        var addressId = customer.Addresses[0].Id;

        _mockCustomerRepository.Setup(x => x.GetByUserIdAsync(_tenantId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        var command = new RemoveCustomerAddressCommand(addressId);

        var handler = new RemoveCustomerAddressCommandHandler(
            _mockCustomerRepository.Object,
            _mockTenantContext.Object,
            _mockCurrentUser.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        customer.Addresses.Should().BeEmpty();
        _mockCustomerRepository.Verify(x => x.UpdateAsync(customer, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithoutTenant_ShouldFail()
    {
        _mockTenantContext.Setup(x => x.TenantId).Returns((Guid?)null);

        var command = new RemoveCustomerAddressCommand(Guid.NewGuid());

        var handler = new RemoveCustomerAddressCommandHandler(
            _mockCustomerRepository.Object,
            _mockTenantContext.Object,
            _mockCurrentUser.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Customer.TenantRequired");
    }

    [Fact]
    public async Task Handle_WithoutUser_ShouldFail()
    {
        _mockCurrentUser.Setup(x => x.UserId).Returns((Guid?)null);

        var command = new RemoveCustomerAddressCommand(Guid.NewGuid());

        var handler = new RemoveCustomerAddressCommandHandler(
            _mockCustomerRepository.Object,
            _mockTenantContext.Object,
            _mockCurrentUser.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Customer.UserRequired");
    }

    [Fact]
    public async Task Handle_WithNonExistentCustomer_ShouldFail()
    {
        _mockCustomerRepository.Setup(x => x.GetByUserIdAsync(_tenantId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        var command = new RemoveCustomerAddressCommand(Guid.NewGuid());

        var handler = new RemoveCustomerAddressCommandHandler(
            _mockCustomerRepository.Object,
            _mockTenantContext.Object,
            _mockCurrentUser.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Customer.NotFound");
    }

    [Fact]
    public async Task Handle_WithNonExistentAddress_ShouldFail()
    {
        var email = Email.Create("joao@test.com").Value;
        var customerResult = Customer.Create(_tenantId, _userId, "João da Silva", email);
        Assert.True(customerResult.IsSuccess);
        var customer = customerResult.Value;

        _mockCustomerRepository.Setup(x => x.GetByUserIdAsync(_tenantId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        var command = new RemoveCustomerAddressCommand(Guid.NewGuid());

        var handler = new RemoveCustomerAddressCommandHandler(
            _mockCustomerRepository.Object,
            _mockTenantContext.Object,
            _mockCurrentUser.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Customer.AddressNotFound");
    }
}

public class SetDefaultCustomerAddressCommandHandlerTests
{
    private readonly Mock<ICustomerRepository> _mockCustomerRepository;
    private readonly Mock<ITenantContext> _mockTenantContext;
    private readonly Mock<ICurrentUser> _mockCurrentUser;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    public SetDefaultCustomerAddressCommandHandlerTests()
    {
        _mockCustomerRepository = new Mock<ICustomerRepository>();
        _mockTenantContext = new Mock<ITenantContext>();
        _mockCurrentUser = new Mock<ICurrentUser>();
        _mockTenantContext.Setup(x => x.TenantId).Returns(_tenantId);
        _mockCurrentUser.Setup(x => x.UserId).Returns(_userId);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldSetDefaultAddress()
    {
        var email = Email.Create("joao@test.com").Value;
        var customerResult = Customer.Create(_tenantId, _userId, "João da Silva", email);
        Assert.True(customerResult.IsSuccess);
        var customer = customerResult.Value;

        var addResult = customer.AddAddress(
            "Casa", "Rua das Flores", "123", null, "Centro",
            "São Paulo", "SP", "01001-000", null, false);
        Assert.True(addResult.IsSuccess);
        var addressId = customer.Addresses[0].Id;

        _mockCustomerRepository.Setup(x => x.GetByUserIdAsync(_tenantId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        var command = new SetDefaultCustomerAddressCommand(addressId);

        var handler = new SetDefaultCustomerAddressCommandHandler(
            _mockCustomerRepository.Object,
            _mockTenantContext.Object,
            _mockCurrentUser.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        customer.Addresses[0].IsDefault.Should().BeTrue();
        _mockCustomerRepository.Verify(x => x.UpdateAsync(customer, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithoutTenant_ShouldFail()
    {
        _mockTenantContext.Setup(x => x.TenantId).Returns((Guid?)null);

        var command = new SetDefaultCustomerAddressCommand(Guid.NewGuid());

        var handler = new SetDefaultCustomerAddressCommandHandler(
            _mockCustomerRepository.Object,
            _mockTenantContext.Object,
            _mockCurrentUser.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Customer.TenantRequired");
    }

    [Fact]
    public async Task Handle_WithoutUser_ShouldFail()
    {
        _mockCurrentUser.Setup(x => x.UserId).Returns((Guid?)null);

        var command = new SetDefaultCustomerAddressCommand(Guid.NewGuid());

        var handler = new SetDefaultCustomerAddressCommandHandler(
            _mockCustomerRepository.Object,
            _mockTenantContext.Object,
            _mockCurrentUser.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Customer.UserRequired");
    }

    [Fact]
    public async Task Handle_WithNonExistentCustomer_ShouldFail()
    {
        _mockCustomerRepository.Setup(x => x.GetByUserIdAsync(_tenantId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        var command = new SetDefaultCustomerAddressCommand(Guid.NewGuid());

        var handler = new SetDefaultCustomerAddressCommandHandler(
            _mockCustomerRepository.Object,
            _mockTenantContext.Object,
            _mockCurrentUser.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Customer.NotFound");
    }

    [Fact]
    public async Task Handle_WithNonExistentAddress_ShouldFail()
    {
        var email = Email.Create("joao@test.com").Value;
        var customerResult = Customer.Create(_tenantId, _userId, "João da Silva", email);
        Assert.True(customerResult.IsSuccess);
        var customer = customerResult.Value;

        _mockCustomerRepository.Setup(x => x.GetByUserIdAsync(_tenantId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        var command = new SetDefaultCustomerAddressCommand(Guid.NewGuid());

        var handler = new SetDefaultCustomerAddressCommandHandler(
            _mockCustomerRepository.Object,
            _mockTenantContext.Object,
            _mockCurrentUser.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Customer.AddressNotFound");
    }
}
