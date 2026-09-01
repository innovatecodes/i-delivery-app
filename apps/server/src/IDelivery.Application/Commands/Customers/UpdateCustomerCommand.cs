using IDelivery.Application.Abstractions.CQRS;

namespace IDelivery.Application.Commands.Customers;

public sealed record UpdateCustomerCommand(
    Guid Id,
    string FullName,
    string Email,
    string? PhoneNumber,
    string? Notes) : ICommand;
