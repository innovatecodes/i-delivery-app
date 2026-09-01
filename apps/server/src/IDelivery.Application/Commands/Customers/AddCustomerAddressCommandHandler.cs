using IDelivery.Application.Abstractions.Authentication;
using IDelivery.Application.Abstractions.CQRS;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.SharedKernel.Common.Result;

namespace IDelivery.Application.Commands.Customers;

public sealed class AddCustomerAddressCommandHandler : ICommandHandler<AddCustomerAddressCommand, Guid>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUser _currentUser;

    public AddCustomerAddressCommandHandler(
        ICustomerRepository customerRepository,
        ITenantContext tenantContext,
        ICurrentUser currentUser)
    {
        _customerRepository = customerRepository;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(AddCustomerAddressCommand command, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantContext.TenantId;
        if (!tenantId.HasValue)
            return Result.Failure<Guid>(new Error("Customer.TenantRequired", "Tenant é obrigatório"));

        if (!_currentUser.UserId.HasValue)
            return Result.Failure<Guid>(new Error("Customer.UserRequired", "Usuário é obrigatório"));

        var customer = await _customerRepository.GetByUserIdAsync(tenantId.Value, _currentUser.UserId.Value, cancellationToken);
        if (customer is null)
            return Result.Failure<Guid>(new Error("Customer.NotFound", "Cliente não encontrado"));

        var addResult = customer.AddAddress(
            command.Label,
            command.Street,
            command.Number,
            command.Complement,
            command.Neighborhood,
            command.City,
            command.State,
            command.ZipCode,
            command.Reference,
            command.IsDefault);

        if (addResult.IsFailure)
            return Result.Failure<Guid>(addResult.Error);

        await _customerRepository.UpdateAsync(customer, cancellationToken);

        var newAddress = customer.Addresses.Last();
        return Result.Success(newAddress.Id);
    }
}
