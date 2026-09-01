using IDelivery.SharedKernel.Common.Result;
using IDelivery.Domain.Common.Entities;

namespace IDelivery.Domain.Orders.Entities;

/// <summary>
/// Entidade que representa um item do pedido (snapshot do produto no momento da compra).
/// Não é Aggregate Root — é gerenciada pelo Order.
/// </summary>
public sealed class OrderItem : Entity
{
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = null!;
    public decimal UnitPrice { get; private set; }
    public string Currency { get; private set; } = null!;
    public int Quantity { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private OrderItem() { }

    private OrderItem(
        Guid id,
        Guid orderId,
        Guid productId,
        string productName,
        decimal unitPrice,
        string currency,
        int quantity) : base(id)
    {
        OrderId = orderId;
        ProductId = productId;
        ProductName = productName;
        UnitPrice = unitPrice;
        Currency = currency;
        Quantity = quantity;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Factory method para criar um novo item do pedido (snapshot).
    /// </summary>
    public static Result<OrderItem> Create(
        Guid orderId,
        Guid productId,
        string productName,
        decimal unitPrice,
        string currency,
        int quantity)
    {
        if (orderId == Guid.Empty)
            return Result.Failure<OrderItem>(new Error("OrderItem.OrderRequired", "Pedido é obrigatório"));

        if (productId == Guid.Empty)
            return Result.Failure<OrderItem>(new Error("OrderItem.ProductRequired", "Produto é obrigatório"));

        if (string.IsNullOrWhiteSpace(productName))
            return Result.Failure<OrderItem>(new Error("OrderItem.ProductNameRequired", "Nome do produto é obrigatório"));

        if (productName.Length > 200)
            return Result.Failure<OrderItem>(new Error("OrderItem.ProductNameTooLong", "Nome do produto deve ter no máximo 200 caracteres"));

        if (unitPrice < 0)
            return Result.Failure<OrderItem>(new Error("OrderItem.InvalidPrice", "Preço não pode ser negativo"));

        if (string.IsNullOrWhiteSpace(currency))
            return Result.Failure<OrderItem>(new Error("OrderItem.CurrencyRequired", "Moeda é obrigatória"));

        if (currency.Length != 3)
            return Result.Failure<OrderItem>(new Error("OrderItem.InvalidCurrency", "Moeda deve ter 3 caracteres"));

        if (quantity <= 0)
            return Result.Failure<OrderItem>(new Error("OrderItem.InvalidQuantity", "Quantidade deve ser maior que zero"));

        var item = new OrderItem(
            Guid.NewGuid(),
            orderId,
            productId,
            productName.Trim(),
            unitPrice,
            currency.ToUpperInvariant(),
            quantity);

        return Result.Success(item);
    }

    /// <summary>
    /// Retorna o subtotal do item (preço * quantidade).
    /// </summary>
    public decimal GetSubtotal()
    {
        return UnitPrice * Quantity;
    }
}