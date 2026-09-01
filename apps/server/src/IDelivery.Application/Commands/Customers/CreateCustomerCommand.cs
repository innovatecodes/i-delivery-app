using IDelivery.Application.Abstractions.CQRS;

namespace IDelivery.Application.Commands.Customers;

public sealed record CreateCustomerCommand(
    Guid UserId,
    string FullName,
    string Email,
    string? PhoneNumber,
    string? Notes) : ICommand<Guid>;
