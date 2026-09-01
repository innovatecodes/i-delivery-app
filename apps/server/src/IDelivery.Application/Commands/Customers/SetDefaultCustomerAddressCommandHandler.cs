using IDelivery.Application.Abstractions.Authentication;
using IDelivery.Application.Abstractions.CQRS;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.SharedKernel.Common.Result;

namespace IDelivery.Application.Commands.Customers;

public sealed class SetDefaultCustomerAddressCommandHandler : ICommandHandler<SetDefaultCustomerAddressCommand>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUser _currentUser;

    public SetDefaultCustomerAddressCommandHandler(
        ICustomerRepository customerRepository,
        ITenantContext tenantContext,
        ICurrentUser currentUser)
    {
        _customerRepository = customerRepository;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(SetDefaultCustomerAddressCommand command, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantContext.TenantId;
        if (!tenantId.HasValue)
            return Result.Failure(new Error("Customer.TenantRequired", "Tenant é obrigatório"));

        if (!_currentUser.UserId.HasValue)
            return Result.Failure(new Error("Customer.UserRequired", "Usuário é obrigatório"));

        var customer = await _customerRepository.GetByUserIdAsync(tenantId.Value, _currentUser.UserId.Value, cancellationToken);
        if (customer is null)
            return Result.Failure(new Error("Customer.NotFound", "Cliente não encontrado"));

        var setResult = customer.SetDefaultAddress(command.AddressId);
        if (setResult.IsFailure)
            return Result.Failure(setResult.Error);

        await _customerRepository.UpdateAsync(customer, cancellationToken);

        return Result.Success();
    }
}
