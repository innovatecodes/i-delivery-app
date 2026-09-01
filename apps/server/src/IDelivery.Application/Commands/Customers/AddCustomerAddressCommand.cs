using IDelivery.Application.Abstractions.CQRS;

namespace IDelivery.Application.Commands.Customers;

public sealed record AddCustomerAddressCommand(
    string Label,
    string Street,
    string Number,
    string? Complement,
    string Neighborhood,
    string City,
    string State,
    string ZipCode,
    string? Reference,
    bool IsDefault) : ICommand<Guid>;
