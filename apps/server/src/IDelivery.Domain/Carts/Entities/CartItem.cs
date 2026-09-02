using IDelivery.SharedKernel.Common.Result;
using IDelivery.Domain.Common.ValueObjects;
using IDelivery.Domain.Common.Entities;

namespace IDelivery.Domain.Carts.Entities;

/// <summary>
/// Entidade que representa um item dentro do carrinho.
/// Não é Aggregate Root — é gerenciada pelo Cart.
/// </summary>
public sealed class CartItem : Entity
{
    public Guid CartId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = null!;
    public Money UnitPrice { get; private set; } = null!;
    public int Quantity { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private CartItem() { }

    private CartItem(
        Guid id,
        Guid cartId,
        Guid productId,
        string productName,
        Money unitPrice,
        int quantity) : base(id)
    {
        CartId = cartId;
        ProductId = productId;
        ProductName = productName;
        UnitPrice = unitPrice;
        Quantity = quantity;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Factory method para criar um novo item do carrinho.
    /// </summary>
    public static Result<CartItem> Create(
        Guid cartId,
        Guid productId,
        string productName,
        Money unitPrice,
        int quantity = 1)
    {
        if (cartId == Guid.Empty)
            return Result.Failure<CartItem>(new Error("CartItem.CartRequired", "Carrinho é obrigatório"));

        if (productId == Guid.Empty)
            return Result.Failure<CartItem>(new Error("CartItem.ProductRequired", "Produto é obrigatório"));

        if (string.IsNullOrWhiteSpace(productName))
            return Result.Failure<CartItem>(new Error("CartItem.ProductNameRequired", "Nome do produto é obrigatório"));

        if (unitPrice.Amount < 0)
            return Result.Failure<CartItem>(new Error("CartItem.InvalidPrice", "Preço não pode ser negativo"));

        if (quantity <= 0)
            return Result.Failure<CartItem>(new Error("CartItem.InvalidQuantity", "Quantidade deve ser maior que zero"));

        var item = new CartItem(
            Guid.NewGuid(),
            cartId,
            productId,
            productName.Trim(),
            unitPrice,
            quantity);

        return Result.Success(item);
    }

    /// <summary>
    /// Incrementa a quantidade do item.
    /// </summary>
    public void IncreaseQuantity(int amount)
    {
        if (amount <= 0) return;
        Quantity += amount;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Atualiza a quantidade do item.
    /// </summary>
    public void UpdateQuantity(int quantity)
    {
        if (quantity <= 0) return;
        Quantity = quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Retorna o subtotal do item (preço * quantidade).
    /// </summary>
    public Money Subtotal => UnitPrice.Multiply(Quantity);
}