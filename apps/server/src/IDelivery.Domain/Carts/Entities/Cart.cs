using IDelivery.SharedKernel.Common.Result;
using IDelivery.Domain.Common.Entities;
using IDelivery.Domain.Carts.Events;

namespace IDelivery.Domain.Carts.Entities;

/// <summary>
/// Aggregate Root do Carrinho.
/// Representa o carrinho de compras de um usuário/sessão dentro de um tenant.
/// Pode ser anônimo (SessionId) ou autenticado (UserId).
/// </summary>
public sealed class Cart : AggregateRoot
{
    public Guid TenantId { get; private set; }
    public Guid? UserId { get; private set; }
    public string? SessionId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private readonly List<CartItem> _items = [];
    public IReadOnlyList<CartItem> Items => _items.AsReadOnly();

    private Cart() { }

    private Cart(
        Guid id,
        Guid tenantId,
        Guid? userId,
        string? sessionId) : base(id)
    {
        TenantId = tenantId;
        UserId = userId;
        SessionId = sessionId;
        CreatedAt = DateTime.UtcNow;

        AddDomainEvent(new CartCreatedDomainEvent(id, tenantId, userId));
    }

    /// <summary>
    /// Factory method para criar um novo carrinho.
    /// </summary>
    public static Result<Cart> Create(
        Guid tenantId,
        Guid? userId = null,
        string? sessionId = null)
    {
        if (tenantId == Guid.Empty)
            return Result.Failure<Cart>(new Error("Cart.TenantRequired", "Tenant é obrigatório"));

        if (userId == null && string.IsNullOrWhiteSpace(sessionId))
            return Result.Failure<Cart>(new Error("Cart.UserOrSessionRequired", "Usuário ou sessão é obrigatório"));

        var cart = new Cart(Guid.NewGuid(), tenantId, userId, sessionId);
        return Result.Success(cart);
    }

    /// <summary>
    /// Adiciona um item ao carrinho.
    /// Se o produto já existe, incrementa a quantidade.
    /// </summary>
    public Result AddItem(
        Guid productId,
        string productName,
        decimal unitPrice,
        string currency,
        int quantity = 1)
    {
        if (quantity <= 0)
            return Result.Failure(new Error("Cart.InvalidQuantity", "Quantidade deve ser maior que zero"));

        if (productId == Guid.Empty)
            return Result.Failure(new Error("Cart.ProductRequired", "Produto é obrigatório"));

        if (string.IsNullOrWhiteSpace(productName))
            return Result.Failure(new Error("Cart.ProductNameRequired", "Nome do produto é obrigatório"));

        if (unitPrice < 0)
            return Result.Failure(new Error("Cart.InvalidPrice", "Preço não pode ser negativo"));

        if (string.IsNullOrWhiteSpace(currency))
            return Result.Failure(new Error("Cart.CurrencyRequired", "Moeda é obrigatória"));

        var existingItem = _items.FirstOrDefault(i => i.ProductId == productId);

        if (existingItem is not null)
        {
            existingItem.IncreaseQuantity(quantity);
        }
        else
        {
            var itemResult = CartItem.Create(Id, productId, productName, unitPrice, currency, quantity);
            if (itemResult.IsFailure)
                return Result.Failure(itemResult.Error);

            _items.Add(itemResult.Value);
        }

        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new CartItemAddedDomainEvent(Id, TenantId, productId, quantity));

        return Result.Success();
    }

    /// <summary>
    /// Remove um item do carrinho.
    /// </summary>
    public Result RemoveItem(Guid productId)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item is null)
            return Result.Failure(new Error("Cart.ItemNotFound", "Item não encontrado no carrinho"));

        _items.Remove(item);
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new CartItemRemovedDomainEvent(Id, TenantId, productId));

        return Result.Success();
    }

    /// <summary>
    /// Atualiza a quantidade de um item.
    /// </summary>
    public Result UpdateItemQuantity(Guid productId, int quantity)
    {
        if (quantity <= 0)
            return Result.Failure(new Error("Cart.InvalidQuantity", "Quantidade deve ser maior que zero"));

        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item is null)
            return Result.Failure(new Error("Cart.ItemNotFound", "Item não encontrado no carrinho"));

        item.UpdateQuantity(quantity);
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    /// <summary>
    /// Limpa todos os itens do carrinho.
    /// </summary>
    public Result Clear()
    {
        if (_items.Count == 0)
            return Result.Failure(new Error("Cart.AlreadyEmpty", "Carrinho já está vazio"));

        _items.Clear();
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new CartClearedDomainEvent(Id, TenantId));

        return Result.Success();
    }

    /// <summary>
    /// Retorna o total do carrinho.
    /// </summary>
    public decimal GetTotal()
    {
        return _items.Sum(i => i.UnitPrice * i.Quantity);
    }

    /// <summary>
    /// Retorna a quantidade total de itens.
    /// </summary>
    public int GetItemCount()
    {
        return _items.Sum(i => i.Quantity);
    }
}
