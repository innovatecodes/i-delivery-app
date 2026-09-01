using IDelivery.SharedKernel.Common.Result;
using IDelivery.Domain.Common.Entities;
using IDelivery.Domain.Orders.Enums;
using IDelivery.Domain.Orders.Events;

namespace IDelivery.Domain.Orders.Entities;

/// <summary>
/// Aggregate Root do Pedido.
/// Representa um pedido de um cliente dentro de um tenant.
/// Possui snapshot dos itens e controle de estados com transições explícitas.
/// </summary>
public sealed class Order : AggregateRoot
{
    public Guid TenantId { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid? DeliveryDriverId { get; private set; }
    public OrderState State { get; private set; }
    public decimal Subtotal { get; private set; }
    public decimal DeliveryFee { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string Currency { get; private set; } = null!;
    public string? DeliveryAddress { get; private set; }
    public decimal? DeliveryDistanceKm { get; private set; }
    public string? DeliveryFailureReasonDetail { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ConfirmedAt { get; private set; }
    public DateTime? PreparingAt { get; private set; }
    public DateTime? ReadyAt { get; private set; }
    public DateTime? OutForDeliveryAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }

    private readonly List<OrderItem> _items = [];
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();

    private Order() { }

    private Order(
        Guid id,
        Guid tenantId,
        Guid customerId,
        List<OrderItem> items,
        decimal deliveryFee,
        string currency,
        string deliveryAddress,
        decimal? deliveryDistanceKm) : base(id)
    {
        TenantId = tenantId;
        CustomerId = customerId;
        State = OrderState.Pending;
        _items.AddRange(items);
        Subtotal = items.Sum(i => i.GetSubtotal());
        DeliveryFee = deliveryFee;
        TotalAmount = Subtotal + deliveryFee;
        Currency = currency;
        DeliveryAddress = deliveryAddress;
        DeliveryDistanceKm = deliveryDistanceKm;
        CreatedAt = DateTime.UtcNow;

        AddDomainEvent(new OrderCreatedDomainEvent(id, tenantId, customerId, TotalAmount, currency));
    }

    /// <summary>
    /// Factory method para criar um novo pedido a partir do carrinho.
    /// </summary>
    public static Result<Order> Create(
        Guid tenantId,
        Guid customerId,
        List<OrderItem> items,
        decimal deliveryFee,
        string currency,
        string deliveryAddress,
        decimal? deliveryDistanceKm = null)
    {
        if (tenantId == Guid.Empty)
            return Result.Failure<Order>(new Error("Order.TenantRequired", "Tenant é obrigatório"));

        if (customerId == Guid.Empty)
            return Result.Failure<Order>(new Error("Order.CustomerRequired", "Cliente é obrigatório"));

        if (items == null || items.Count == 0)
            return Result.Failure<Order>(new Error("Order.ItemsRequired", "Pedido deve ter pelo menos um item"));

        if (deliveryFee < 0)
            return Result.Failure<Order>(new Error("Order.InvalidDeliveryFee", "Taxa de entrega não pode ser negativa"));

        if (string.IsNullOrWhiteSpace(currency))
            return Result.Failure<Order>(new Error("Order.CurrencyRequired", "Moeda é obrigatória"));

        if (currency.Length != 3)
            return Result.Failure<Order>(new Error("Order.InvalidCurrency", "Moeda deve ter 3 caracteres"));

        if (string.IsNullOrWhiteSpace(deliveryAddress))
            return Result.Failure<Order>(new Error("Order.AddressRequired", "Endereço de entrega é obrigatório"));

        var order = new Order(
            Guid.NewGuid(),
            tenantId,
            customerId,
            items,
            deliveryFee,
            currency.ToUpperInvariant(),
            deliveryAddress.Trim(),
            deliveryDistanceKm);

        return Result.Success(order);
    }

    /// <summary>
    /// Confirma o pedido (Tenant aceita).
    /// PENDING → CONFIRMED
    /// </summary>
    public Result Confirm()
    {
        if (State != OrderState.Pending)
            return Result.Failure(new Error("Order.InvalidTransition", "Pedido só pode ser confirmado a partir do estado PENDING"));

        State = OrderState.Confirmed;
        ConfirmedAt = DateTime.UtcNow;

        AddDomainEvent(new OrderStatusChangedDomainEvent(Id, TenantId, OrderState.Pending, OrderState.Confirmed));

        return Result.Success();
    }

    /// <summary>
    /// Inicia o preparo do pedido.
    /// CONFIRMED → PREPARING
    /// </summary>
    public Result StartPreparing()
    {
        if (State != OrderState.Confirmed)
            return Result.Failure(new Error("Order.InvalidTransition", "Pedido só pode iniciar preparo a partir do estado CONFIRMED"));

        State = OrderState.Preparing;
        PreparingAt = DateTime.UtcNow;

        AddDomainEvent(new OrderStatusChangedDomainEvent(Id, TenantId, OrderState.Confirmed, OrderState.Preparing));

        return Result.Success();
    }

    /// <summary>
    /// Marca o pedido como pronto para entrega.
    /// PREPARING → READY_FOR_DELIVERY
    /// </summary>
    public Result MarkReadyForDelivery()
    {
        if (State != OrderState.Preparing)
            return Result.Failure(new Error("Order.InvalidTransition", "Pedido só pode ser marcado como pronto a partir do estado PREPARING"));

        State = OrderState.ReadyForDelivery;
        ReadyAt = DateTime.UtcNow;

        AddDomainEvent(new OrderStatusChangedDomainEvent(Id, TenantId, OrderState.Preparing, OrderState.ReadyForDelivery));

        return Result.Success();
    }

    /// <summary>
    /// Atribui entregador e inicia entrega.
    /// READY_FOR_DELIVERY → OUT_FOR_DELIVERY
    /// </summary>
    public Result StartDelivery(Guid deliveryDriverId)
    {
        if (State != OrderState.ReadyForDelivery)
            return Result.Failure(new Error("Order.InvalidTransition", "Pedido só pode sair para entrega a partir do estado READY_FOR_DELIVERY"));

        if (deliveryDriverId == Guid.Empty)
            return Result.Failure(new Error("Order.DriverRequired", "Entregador é obrigatório"));

        State = OrderState.OutForDelivery;
        DeliveryDriverId = deliveryDriverId;
        OutForDeliveryAt = DateTime.UtcNow;

        AddDomainEvent(new OrderStatusChangedDomainEvent(Id, TenantId, OrderState.ReadyForDelivery, OrderState.OutForDelivery));

        return Result.Success();
    }

    /// <summary>
    /// Confirma entrega realizada com sucesso.
    /// OUT_FOR_DELIVERY → DELIVERED
    /// Apenas entregador atribuído pode confirmar.
    /// </summary>
    public Result Deliver(Guid deliveryDriverId)
    {
        if (State != OrderState.OutForDelivery)
            return Result.Failure(new Error("Order.InvalidTransition", "Entrega só pode ser confirmada a partir do estado OUT_FOR_DELIVERY"));

        if (DeliveryDriverId != deliveryDriverId)
            return Result.Failure(new Error("Order.UnauthorizedDriver", "Apenas o entregador atribuído pode confirmar a entrega"));

        State = OrderState.Delivered;
        CompletedAt = DateTime.UtcNow;

        AddDomainEvent(new OrderStatusChangedDomainEvent(Id, TenantId, OrderState.OutForDelivery, OrderState.Delivered));
        AddDomainEvent(new OrderDeliveredDomainEvent(Id, TenantId, CustomerId, DeliveryDriverId!.Value));

        return Result.Success();
    }

    /// <summary>
    /// Confirma falha na entrega.
    /// OUT_FOR_DELIVERY → DELIVERY_FAILED
    /// Apenas entregador atribuído pode confirmar. Exige motivo obrigatório.
    /// </summary>
    public Result FailDelivery(Guid deliveryDriverId, DeliveryFailureReason reason, string? reasonDetail = null)
    {
        if (State != OrderState.OutForDelivery)
            return Result.Failure(new Error("Order.InvalidTransition", "Falha de entrega só pode ser confirmada a partir do estado OUT_FOR_DELIVERY"));

        if (DeliveryDriverId != deliveryDriverId)
            return Result.Failure(new Error("Order.UnauthorizedDriver", "Apenas o entregador atribuído pode confirmar falha na entrega"));

        if (reason == DeliveryFailureReason.Other && string.IsNullOrWhiteSpace(reasonDetail))
            return Result.Failure(new Error("Order.FailureReasonDetailRequired", "Detalhe do motivo é obrigatório quando o motivo é 'Outro'"));

        State = OrderState.DeliveryFailed;
        CompletedAt = DateTime.UtcNow;
        DeliveryFailureReasonDetail = reasonDetail?.Trim();

        AddDomainEvent(new OrderStatusChangedDomainEvent(Id, TenantId, OrderState.OutForDelivery, OrderState.DeliveryFailed));
        AddDomainEvent(new OrderDeliveryFailedDomainEvent(Id, TenantId, CustomerId, DeliveryDriverId!.Value, reason, reasonDetail));

        return Result.Success();
    }

    /// <summary>
    /// Cancela o pedido.
    /// Permitido a partir de: PENDING, CONFIRMED, PREPARING, READY_FOR_DELIVERY
    /// NÃO permitido a partir de: OUT_FOR_DELIVERY, DELIVERED, DELIVERY_FAILED, CANCELLED
    /// Autoridade: Tenant, Cliente ou Sistema (não entregador).
    /// </summary>
    public Result Cancel(string? cancelledBy = null)
    {
        var allowedStates = new[] { OrderState.Pending, OrderState.Confirmed, OrderState.Preparing, OrderState.ReadyForDelivery };

        if (!allowedStates.Contains(State))
            return Result.Failure(new Error("Order.InvalidTransition", "Pedido não pode ser cancelado a partir do estado atual"));

        if (State == OrderState.Cancelled)
            return Result.Failure(new Error("Order.AlreadyCancelled", "Pedido já está cancelado"));

        State = OrderState.Cancelled;
        CancelledAt = DateTime.UtcNow;

        AddDomainEvent(new OrderStatusChangedDomainEvent(Id, TenantId, State, OrderState.Cancelled));
        AddDomainEvent(new OrderCancelledDomainEvent(Id, TenantId, CustomerId, cancelledBy));

        return Result.Success();
    }

    /// <summary>
    /// Verifica se o pedido pode ser cancelado pelo estado atual.
    /// </summary>
    public bool CanBeCancelled()
    {
        return State is OrderState.Pending or OrderState.Confirmed or OrderState.Preparing or OrderState.ReadyForDelivery;
    }

    /// <summary>
    /// Verifica se o pedido está em estado terminal.
    /// </summary>
    public bool IsTerminalState()
    {
        return State is OrderState.Delivered or OrderState.DeliveryFailed or OrderState.Cancelled;
    }
}