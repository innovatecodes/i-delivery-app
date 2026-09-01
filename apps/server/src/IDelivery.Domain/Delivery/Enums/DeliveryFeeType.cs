namespace IDelivery.Domain.Delivery.Enums;

/// <summary>
/// Tipos de taxa de entrega disponíveis.
/// </summary>
public enum DeliveryFeeType
{
    /// <summary>
    /// Entrega totalmente gratuita.
    /// </summary>
    Free = 0,

    /// <summary>
    /// Entrega grátis acima de um valor mínimo do pedido.
    /// </summary>
    FreeAboveAmount = 1,

    /// <summary>
    /// Taxa fixa independentemente do valor ou distância.
    /// </summary>
    Fixed = 2,

    /// <summary>
    /// Taxa calculada por distância (km).
    /// </summary>
    PerDistance = 3
}