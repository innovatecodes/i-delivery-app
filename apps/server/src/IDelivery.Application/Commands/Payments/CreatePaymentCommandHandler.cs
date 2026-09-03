using IDelivery.Application.Abstractions.CQRS;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.Domain.Common.ValueObjects;
using IDelivery.Domain.Payments.Entities;
using IDelivery.SharedKernel.Common.Result;

namespace IDelivery.Application.Commands.Payments;

public sealed class CreatePaymentCommandHandler : ICommandHandler<CreatePaymentCommand, Guid>
{
    private readonly IPaymentRepository _paymentRepository;

    public CreatePaymentCommandHandler(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<Result<Guid>> Handle(CreatePaymentCommand command, CancellationToken cancellationToken = default)
    {
        var existingPayment = await _paymentRepository.GetByOrderIdAsync(command.OrderId, cancellationToken);
        if (existingPayment is not null)
            return Result.Failure<Guid>(new Error("Payment.AlreadyExists", "Já existe um pagamento para este pedido"));

        var amountResult = Money.Create(command.Amount, command.Currency);
        if (amountResult.IsFailure)
            return Result.Failure<Guid>(amountResult.Error);

        var paymentResult = Payment.Create(
            command.OrderId,
            command.TenantId,
            command.CustomerId,
            amountResult.Value,
            command.Method);

        if (paymentResult.IsFailure)
            return Result.Failure<Guid>(paymentResult.Error);

        await _paymentRepository.AddAsync(paymentResult.Value, cancellationToken);

        return Result.Success(paymentResult.Value.Id);
    }
}
