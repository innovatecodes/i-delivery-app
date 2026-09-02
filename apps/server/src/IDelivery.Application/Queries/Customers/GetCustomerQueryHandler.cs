using IDelivery.Application.Abstractions.Authentication;
using IDelivery.Application.Abstractions.CQRS;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.Domain.Common.ValueObjects;
using IDelivery.SharedKernel.Common.Result;

namespace IDelivery.Application.Queries.Customers;

public sealed class GetCustomerQueryHandler : IQueryHandler<GetCustomerQuery, CustomerResponse>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUser _currentUser;

    public GetCustomerQueryHandler(
        ICustomerRepository customerRepository,
        ITenantContext tenantContext,
        ICurrentUser currentUser)
    {
        _customerRepository = customerRepository;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
    }

    public async Task<Result<CustomerResponse>> Handle(GetCustomerQuery query, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantContext.TenantId;
        if (!tenantId.HasValue)
            return Result.Failure<CustomerResponse>(new Error("Customer.TenantRequired", "Tenant é obrigatório"));

        if (!_currentUser.UserId.HasValue)
            return Result.Failure<CustomerResponse>(new Error("Customer.UserRequired", "Usuário é obrigatório"));

        var customer = await _customerRepository.GetByUserIdAsync(tenantId.Value, _currentUser.UserId.Value, cancellationToken);
        if (customer is null)
            return Result.Failure<CustomerResponse>(new Error("Customer.NotFound", "Cliente não encontrado"));

        var response = new CustomerResponse(
            customer.Id,
            customer.TenantId,
            customer.UserId,
            customer.FullName,
            customer.Email.Value,
            customer.PhoneNumber?.ToString(),
            customer.Notes,
            customer.IsActive,
            customer.Addresses.Select(a => new CustomerAddressResponse(
                a.Id,
                a.Label,
                a.Address.Street,
                a.Address.Number,
                a.Address.Complement,
                a.Address.Neighborhood,
                a.Address.City,
                a.Address.State,
                a.Address.ZipCode.Value,
                a.Address.Reference,
                a.IsDefault,
                a.CreatedAt)).ToList(),
            customer.CreatedAt,
            customer.UpdatedAt);

        return Result.Success(response);
    }
}