using IDelivery.Application.Abstractions.Authentication;
using IDelivery.Application.Abstractions.CQRS;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.Domain.Carts.Entities;
using IDelivery.SharedKernel.Common.Result;

namespace IDelivery.Application.Commands.Carts;

public sealed class AddCartItemCommandHandler : ICommandHandler<AddCartItemCommand>
{
    private readonly ICartRepository _cartRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUser _currentUser;

    public AddCartItemCommandHandler(
        ICartRepository cartRepository,
        ITenantContext tenantContext,
        ICurrentUser currentUser)
    {
        _cartRepository = cartRepository;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(AddCartItemCommand command, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantContext.TenantId;
        if (!tenantId.HasValue)
            return Result.Failure(new Error("Cart.TenantRequired", "Tenant é obrigatório"));

        Cart? cart;

        if (_currentUser.UserId.HasValue)
        {
            cart = await _cartRepository.GetByUserIdAsync(tenantId.Value, _currentUser.UserId.Value, cancellationToken);
        }
        else
        {
            return Result.Failure(new Error("Cart.UserRequired", "Usuário é obrigatório"));
        }

        if (cart is null)
        {
            var cartResult = Cart.Create(tenantId.Value, _currentUser.UserId);
            if (cartResult.IsFailure)
                return Result.Failure(cartResult.Error);

            cart = cartResult.Value;
            await _cartRepository.AddAsync(cart, cancellationToken);
        }

        var addResult = cart.AddItem(
            command.ProductId,
            command.ProductName,
            command.UnitPrice,
            command.Currency,
            command.Quantity);

        if (addResult.IsFailure)
            return Result.Failure(addResult.Error);

        await _cartRepository.UpdateAsync(cart, cancellationToken);

        return Result.Success();
    }
}
