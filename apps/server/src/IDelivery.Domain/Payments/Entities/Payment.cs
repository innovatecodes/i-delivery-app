using IDelivery.Domain.Common.Entities;
using IDelivery.Domain.Common.ValueObjects;
using IDelivery.Domain.Payments.Enums;
using IDelivery.Domain.Payments.Events;
using IDelivery.SharedKernel.Common.Result;

namespace IDelivery.Domain.Payments.Entities;

public sealed class Payment : AggregateRoot
{
    public Guid OrderId { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid CustomerId { get; private set; }
    public Money Amount { get; private set; } = null!;
    public PaymentMethod Method { get; private set; }
    public PaymentStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? PaidAt { get; private set; }

    private Payment() { }

    private Payment(
        Guid id,
        Guid orderId,
        Guid tenantId,
        Guid customerId,
        Money amount,
        PaymentMethod method) : base(id)
    {
        OrderId = orderId;
        TenantId = tenantId;
        CustomerId = customerId;
        Amount = amount;
        Method = method;
        Status = PaymentStatus.Pending;
        CreatedAt = DateTime.UtcNow;

        AddDomainEvent(new PaymentCreatedDomainEvent(id, orderId, tenantId, customerId, amount, method));
    }

    public static Result<Payment> Create(
        Guid orderId,
        Guid tenantId,
        Guid customerId,
        Money amount,
        PaymentMethod method)
    {
        if (orderId == Guid.Empty)
            return Result.Failure<Payment>(new Error("Payment.OrderRequired", "Pedido é obrigatório"));
        if (tenantId == Guid.Empty)
            return Result.Failure<Payment>(new Error("Payment.TenantRequired", "Tenant é obrigatório"));
        if (customerId == Guid.Empty)
            return Result.Failure<Payment>(new Error("Payment.CustomerRequired", "Cliente é obrigatório"));
        if (amount.Amount <= 0)
            return Result.Failure<Payment>(new Error("Payment.InvalidAmount", "Valor do pagamento deve ser maior que zero"));

        var payment = new Payment(Guid.NewGuid(), orderId, tenantId, customerId, amount, method);
        return Result.Success(payment);
    }

    public Result MarkAsPaid()
    {
        if (Status != PaymentStatus.Pending)
            return Result.Failure(new Error("Payment.InvalidStatus", "Apenas pagamentos pendentes podem ser marcados como pagos"));

        Status = PaymentStatus.Paid;
        PaidAt = DateTime.UtcNow;

        AddDomainEvent(new PaymentMarkedAsPaidDomainEvent(Id, OrderId, TenantId, CustomerId));

        return Result.Success();
    }

    public Result MarkAsNotCollected()
    {
        if (Status != PaymentStatus.Pending)
            return Result.Failure(new Error("Payment.InvalidStatus", "Apenas pagamentos pendentes podem ser marcados como não coletados"));

        Status = PaymentStatus.NotCollected;

        return Result.Success();
    }
}
