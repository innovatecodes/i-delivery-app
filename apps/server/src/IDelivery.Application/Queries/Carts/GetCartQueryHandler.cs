using IDelivery.Application.Abstractions.Authentication;
using IDelivery.Application.Abstractions.CQRS;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.Domain.Carts.Entities;
using IDelivery.SharedKernel.Common.Result;

namespace IDelivery.Application.Queries.Carts;

public sealed class GetCartQueryHandler : IQueryHandler<GetCartQuery, CartResponse>
{
    private readonly ICartRepository _cartRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUser _currentUser;

    public GetCartQueryHandler(
        ICartRepository cartRepository,
        ITenantContext tenantContext,
        ICurrentUser currentUser)
    {
        _cartRepository = cartRepository;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
    }

    public async Task<Result<CartResponse>> Handle(GetCartQuery query, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantContext.TenantId;
        if (!tenantId.HasValue)
            return Result.Failure<CartResponse>(new Error("Cart.TenantRequired", "Tenant é obrigatório"));

        if (!_currentUser.UserId.HasValue)
            return Result.Failure<CartResponse>(new Error("Cart.UserRequired", "Usuário é obrigatório"));

        var cart = await _cartRepository.GetByUserIdAsync(tenantId.Value, _currentUser.UserId.Value, cancellationToken);
        if (cart is null)
        {
            var newCartResult = Cart.Create(tenantId.Value, _currentUser.UserId);
            if (newCartResult.IsFailure)
                return Result.Failure<CartResponse>(newCartResult.Error);

            cart = newCartResult.Value;
            await _cartRepository.AddAsync(cart, cancellationToken);
        }

        var response = new CartResponse(
            cart.Id,
            cart.TenantId,
            cart.UserId,
            cart.SessionId,
            cart.Items.Select(i => new CartItemResponse(
                i.Id,
                i.ProductId,
                i.ProductName,
                i.UnitPrice,
                i.Currency,
                i.Quantity,
                i.GetSubtotal())).ToList(),
            cart.GetTotal(),
            cart.GetItemCount(),
            cart.CreatedAt,
            cart.UpdatedAt);

        return Result.Success(response);
    }
}
