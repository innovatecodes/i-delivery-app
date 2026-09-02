namespace IDelivery.Application.Queries.Carts;

public sealed record CartResponse(
    Guid Id,
    Guid TenantId,
    Guid? UserId,
    string? SessionId,
    List<CartItemResponse> Items,
    decimal TotalAmount,
    string Currency,
    int TotalItems,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record CartItemResponse(
    Guid Id,
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    string Currency,
    int Quantity,
    decimal Subtotal);