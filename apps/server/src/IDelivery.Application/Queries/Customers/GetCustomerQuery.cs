using IDelivery.Application.Abstractions.CQRS;

namespace IDelivery.Application.Queries.Customers;

public sealed record GetCustomerQuery : IQuery<CustomerResponse>;
