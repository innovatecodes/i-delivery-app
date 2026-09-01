namespace IDelivery.Domain.Orders.Enums;

/// <summary>
/// Motivos possíveis para falha na entrega.
/// Usado quando o entregador confirma DELIVERY_FAILED.
/// </summary>
public enum DeliveryFailureReason
{
    /// <summary>
    /// Cliente não estava no local.
    /// </summary>
    CustomerAbsent = 0,

    /// <summary>
    /// Endereço não encontrado / incorreto.
    /// </summary>
    AddressNotFound = 1,

    /// <summary>
    /// Cliente recusou o pedido no ato.
    /// </summary>
    CustomerRefused = 2,

    /// <summary>
    /// Outro motivo (campo de texto livre complementar obrigatório).
    /// </summary>
    Other = 3
}