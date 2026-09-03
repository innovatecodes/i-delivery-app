using IDelivery.Application.Abstractions.CQRS;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.SharedKernel.Common.Result;

namespace IDelivery.Application.Commands.Payments;

public sealed class MarkPaymentAsPaidCommandHandler : ICommandHandler<MarkPaymentAsPaidCommand>
{
    private readonly IPaymentRepository _paymentRepository;

    public MarkPaymentAsPaidCommandHandler(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<Result> Handle(MarkPaymentAsPaidCommand command, CancellationToken cancellationToken = default)
    {
        var payment = await _paymentRepository.GetByIdAsync(command.PaymentId, cancellationToken);
        if (payment is null)
            return Result.Failure(new Error("Payment.NotFound", "Pagamento não encontrado"));

        var result = payment.MarkAsPaid();
        if (result.IsFailure)
            return result;

        _paymentRepository.Update(payment);

        return Result.Success();
    }
}
