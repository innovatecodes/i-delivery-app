using IDelivery.Application.Abstractions.Events;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.Domain.Orders.Events;
using Microsoft.Extensions.Logging;

namespace IDelivery.Application.Events.Handlers;

public sealed class OrderDeliveredPaymentHandler : IDomainEventHandler<OrderDeliveredDomainEvent>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILogger<OrderDeliveredPaymentHandler> _logger;

    public OrderDeliveredPaymentHandler(
        IPaymentRepository paymentRepository,
        ILogger<OrderDeliveredPaymentHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _logger = logger;
    }

    public async Task Handle(OrderDeliveredDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var payment = await _paymentRepository.GetByOrderIdAsync(domainEvent.OrderId, cancellationToken);

        if (payment is null)
        {
            _logger.LogWarning(
                "Nenhum pagamento encontrado para o pedido {OrderId} após entrega",
                domainEvent.OrderId);
            return;
        }

        var result = payment.MarkAsPaid();

        if (result.IsFailure)
        {
            _logger.LogWarning(
                "Não foi possível marcar pagamento {PaymentId} como pago: {Error}",
                payment.Id,
                result.Error.Code);
            return;
        }

        _paymentRepository.Update(payment);

        _logger.LogInformation(
            "Pagamento {PaymentId} marcado como pago para o pedido {OrderId}",
            payment.Id,
            domainEvent.OrderId);
    }
}
