using IDelivery.Application.Abstractions.CQRS;

namespace IDelivery.Application.Commands.Customers;

public sealed record DeleteCustomerCommand(
    Guid Id) : ICommand;
