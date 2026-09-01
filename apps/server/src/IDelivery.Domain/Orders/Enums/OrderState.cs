namespace IDelivery.Domain.Orders.Enums;

/// <summary>
/// Estados possíveis de um pedido.
/// Fluxo: PENDING → CONFIRMED → PREPARING → READY_FOR_DELIVERY → OUT_FOR_DELIVERY → DELIVERED | DELIVERY_FAILED
/// CANCELLED é estado terminal à parte, acessível apenas de estados permitidos (não a partir de OUT_FOR_DELIVERY).
/// </summary>
public enum OrderState
{
    /// <summary>
    /// Pedido criado, aguardando confirmação do tenant.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Pedido confirmado pelo tenant, aguardando preparo.
    /// </summary>
    Confirmed = 1,

    /// <summary>
    /// Pedido em preparo.
    /// </summary>
    Preparing = 2,

    /// <summary>
    /// Pedido pronto, aguardando atribuição/retirada pelo entregador.
    /// </summary>
    ReadyForDelivery = 3,

    /// <summary>
    /// Entregador saiu para entrega.
    /// </summary>
    OutForDelivery = 4,

    /// <summary>
    /// Entrega realizada com sucesso (estado terminal de sucesso).
    /// Apenas entregador pode confirmar.
    /// </summary>
    Delivered = 5,

    /// <summary>
    /// Tentativa de entrega falhou (estado terminal de falha).
    /// Apenas entregador pode confirmar. Exige motivo obrigatório.
    /// </summary>
    DeliveryFailed = 6,

    /// <summary>
    /// Pedido cancelado (estado terminal à parte).
    /// Autoridade: Tenant, Cliente ou Sistema. Entregador NÃO pode cancelar.
    /// Não permitido a partir de OUT_FOR_DELIVERY.
    /// </summary>
    Cancelled = 7
}