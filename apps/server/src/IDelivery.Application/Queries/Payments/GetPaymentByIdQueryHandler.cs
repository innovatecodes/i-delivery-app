using IDelivery.Application.Abstractions.CQRS;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.Application.Queries.Payments;
using IDelivery.SharedKernel.Common.Result;

namespace IDelivery.Application.Queries.Payments;

public sealed class GetPaymentByIdQueryHandler : IQueryHandler<GetPaymentByIdQuery, PaymentResponse>
{
    private readonly IPaymentRepository _paymentRepository;

    public GetPaymentByIdQueryHandler(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<Result<PaymentResponse>> Handle(GetPaymentByIdQuery query, CancellationToken cancellationToken = default)
    {
        var payment = await _paymentRepository.GetByIdAsync(query.PaymentId, cancellationToken);
        if (payment is null)
            return Result.Failure<PaymentResponse>(new Error("Payment.NotFound", "Pagamento não encontrado"));

        var response = new PaymentResponse(
            payment.Id,
            payment.OrderId,
            payment.TenantId,
            payment.CustomerId,
            payment.Amount,
            payment.Amount.Currency,
            payment.Method,
            payment.Status,
            payment.CreatedAt,
            payment.PaidAt);

        return Result.Success(response);
    }
}
