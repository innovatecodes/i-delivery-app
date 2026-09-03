using IDelivery.Application.Abstractions.CQRS;
using IDelivery.Domain.Payments.Enums;

namespace IDelivery.Application.Commands.Payments;

public sealed record CreatePaymentCommand(
    Guid OrderId,
    Guid TenantId,
    Guid CustomerId,
    decimal Amount,
    string Currency,
    PaymentMethod Method) : ICommand<Guid>;

public sealed record MarkPaymentAsPaidCommand(Guid PaymentId) : ICommand;

public sealed record MarkPaymentAsNotCollectedCommand(Guid PaymentId) : ICommand;
