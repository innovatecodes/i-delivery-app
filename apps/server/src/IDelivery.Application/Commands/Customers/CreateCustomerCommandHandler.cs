using IDelivery.Application.Abstractions.Authentication;
using IDelivery.Application.Abstractions.CQRS;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.Domain.Common.ValueObjects;
using IDelivery.Domain.Customers.Entities;
using IDelivery.SharedKernel.Common.Result;

namespace IDelivery.Application.Commands.Customers;

public sealed class CreateCustomerCommandHandler : ICommandHandler<CreateCustomerCommand, Guid>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ITenantContext _tenantContext;

    public CreateCustomerCommandHandler(
        ICustomerRepository customerRepository,
        ITenantContext tenantContext)
    {
        _customerRepository = customerRepository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<Guid>> Handle(CreateCustomerCommand command, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantContext.TenantId;
        if (!tenantId.HasValue)
            return Result.Failure<Guid>(new Error("Customer.TenantRequired", "Tenant é obrigatório"));

        if (await _customerRepository.ExistsByEmailAsync(tenantId.Value, command.Email, cancellationToken: cancellationToken))
            return Result.Failure<Guid>(new Error("Customer.EmailAlreadyExists", "Já existe um cliente com este email"));

        var emailResult = Email.Create(command.Email);
        if (emailResult.IsFailure)
            return Result.Failure<Guid>(emailResult.Error);

        var email = emailResult.Value;

        PhoneNumber? phoneNumber = null;
        if (!string.IsNullOrWhiteSpace(command.PhoneNumber))
        {
            var phoneResult = PhoneNumber.Create(command.PhoneNumber);
            if (phoneResult.IsFailure)
                return Result.Failure<Guid>(phoneResult.Error);
            phoneNumber = phoneResult.Value;
        }

        var customerResult = Customer.Create(
            tenantId.Value,
            command.UserId,
            command.FullName,
            email,
            phoneNumber,
            command.Notes);

        if (customerResult.IsFailure)
            return Result.Failure<Guid>(customerResult.Error);

        var customer = customerResult.Value;
        await _customerRepository.AddAsync(customer, cancellationToken);

        return Result.Success(customer.Id);
    }
}
