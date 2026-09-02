using IDelivery.Application.Abstractions.Authentication;
using IDelivery.Application.Abstractions.CQRS;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.Domain.Common.ValueObjects;
using IDelivery.SharedKernel.Common.Result;

namespace IDelivery.Application.Commands.Customers;

public sealed class UpdateCustomerCommandHandler : ICommandHandler<UpdateCustomerCommand>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ITenantContext _tenantContext;

    public UpdateCustomerCommandHandler(
        ICustomerRepository customerRepository,
        ITenantContext tenantContext)
    {
        _customerRepository = customerRepository;
        _tenantContext = tenantContext;
    }

    public async Task<Result> Handle(UpdateCustomerCommand command, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantContext.TenantId;
        if (!tenantId.HasValue)
            return Result.Failure(new Error("Customer.TenantRequired", "Tenant é obrigatório"));

        var customer = await _customerRepository.GetByIdAsync(command.Id, cancellationToken);
        if (customer is null)
            return Result.Failure(new Error("Customer.NotFound", "Cliente não encontrado"));

        if (customer.TenantId != tenantId.Value)
            return Result.Failure(new Error("Customer.AccessDenied", "Acesso negado"));

        if (await _customerRepository.ExistsByEmailAsync(tenantId.Value, command.Email, command.Id, cancellationToken))
            return Result.Failure(new Error("Customer.EmailAlreadyExists", "Já existe um cliente com este email"));

        var emailResult = Email.Create(command.Email);
        if (emailResult.IsFailure)
            return Result.Failure(emailResult.Error);

        var email = emailResult.Value;

        PhoneNumber? phoneNumber = null;
        if (!string.IsNullOrWhiteSpace(command.PhoneNumber))
        {
            var phoneResult = PhoneNumber.Create(command.PhoneNumber);
            if (phoneResult.IsFailure)
                return Result.Failure(phoneResult.Error);
            phoneNumber = phoneResult.Value;
        }

        var updateResult = customer.UpdateProfile(
            command.FullName,
            email,
            phoneNumber,
            command.Notes);

        if (updateResult.IsFailure)
            return Result.Failure(updateResult.Error);

        await _customerRepository.UpdateAsync(customer, cancellationToken);

        return Result.Success();
    }
}
