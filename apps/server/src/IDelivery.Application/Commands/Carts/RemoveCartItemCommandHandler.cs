using IDelivery.Application.Abstractions.Authentication;
using IDelivery.Application.Abstractions.CQRS;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.SharedKernel.Common.Result;

namespace IDelivery.Application.Commands.Carts;

public sealed class RemoveCartItemCommandHandler : ICommandHandler<RemoveCartItemCommand>
{
    private readonly ICartRepository _cartRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUser _currentUser;

    public RemoveCartItemCommandHandler(
        ICartRepository cartRepository,
        ITenantContext tenantContext,
        ICurrentUser currentUser)
    {
        _cartRepository = cartRepository;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(RemoveCartItemCommand command, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantContext.TenantId;
        if (!tenantId.HasValue)
            return Result.Failure(new Error("Cart.TenantRequired", "Tenant é obrigatório"));

        if (!_currentUser.UserId.HasValue)
            return Result.Failure(new Error("Cart.UserRequired", "Usuário é obrigatório"));

        var cart = await _cartRepository.GetByUserIdAsync(tenantId.Value, _currentUser.UserId.Value, cancellationToken);
        if (cart is null)
            return Result.Failure(new Error("Cart.NotFound", "Carrinho não encontrado"));

        var removeResult = cart.RemoveItem(command.ProductId);
        if (removeResult.IsFailure)
            return Result.Failure(removeResult.Error);

        await _cartRepository.UpdateAsync(cart, cancellationToken);

        return Result.Success();
    }
}
