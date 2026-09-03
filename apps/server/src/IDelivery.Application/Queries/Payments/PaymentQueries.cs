using IDelivery.Application.Abstractions.CQRS;
using IDelivery.Domain.Payments.Enums;

namespace IDelivery.Application.Queries.Payments;

public sealed record GetPaymentByOrderIdQuery(Guid OrderId) : IQuery<PaymentResponse>;

public sealed record GetPaymentByIdQuery(Guid PaymentId) : IQuery<PaymentResponse>;

public sealed record PaymentResponse(
    Guid Id,
    Guid OrderId,
    Guid TenantId,
    Guid CustomerId,
    decimal Amount,
    string Currency,
    PaymentMethod Method,
    PaymentStatus Status,
    DateTime CreatedAt,
    DateTime? PaidAt);
