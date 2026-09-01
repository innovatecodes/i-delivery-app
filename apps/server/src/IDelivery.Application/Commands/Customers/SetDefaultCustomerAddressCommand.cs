using IDelivery.Application.Abstractions.CQRS;

namespace IDelivery.Application.Commands.Customers;

public sealed record SetDefaultCustomerAddressCommand(
    Guid AddressId) : ICommand;
