using IDelivery.Application.Abstractions.Authentication;
using IDelivery.Application.Abstractions.CQRS;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.SharedKernel.Common.Result;

namespace IDelivery.Application.Commands.Customers;

public sealed class DeleteCustomerCommandHandler : ICommandHandler<DeleteCustomerCommand>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ITenantContext _tenantContext;

    public DeleteCustomerCommandHandler(
        ICustomerRepository customerRepository,
        ITenantContext tenantContext)
    {
        _customerRepository = customerRepository;
        _tenantContext = tenantContext;
    }

    public async Task<Result> Handle(DeleteCustomerCommand command, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantContext.TenantId;
        if (!tenantId.HasValue)
            return Result.Failure(new Error("Customer.TenantRequired", "Tenant é obrigatório"));

        var customer = await _customerRepository.GetByIdAsync(command.Id, cancellationToken);
        if (customer is null)
            return Result.Failure(new Error("Customer.NotFound", "Cliente não encontrado"));

        if (customer.TenantId != tenantId.Value)
            return Result.Failure(new Error("Customer.AccessDenied", "Acesso negado"));

        var deactivateResult = customer.Deactivate();
        if (deactivateResult.IsFailure)
            return Result.Failure(deactivateResult.Error);

        await _customerRepository.UpdateAsync(customer, cancellationToken);

        return Result.Success();
    }
}
