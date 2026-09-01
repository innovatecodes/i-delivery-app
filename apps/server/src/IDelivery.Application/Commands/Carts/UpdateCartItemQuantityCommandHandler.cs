using IDelivery.Application.Abstractions.Authentication;
using IDelivery.Application.Abstractions.CQRS;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.SharedKernel.Common.Result;

namespace IDelivery.Application.Commands.Carts;

public sealed class UpdateCartItemQuantityCommandHandler : ICommandHandler<UpdateCartItemQuantityCommand>
{
    private readonly ICartRepository _cartRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUser _currentUser;

    public UpdateCartItemQuantityCommandHandler(
        ICartRepository cartRepository,
        ITenantContext tenantContext,
        ICurrentUser currentUser)
    {
        _cartRepository = cartRepository;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(UpdateCartItemQuantityCommand command, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantContext.TenantId;
        if (!tenantId.HasValue)
            return Result.Failure(new Error("Cart.TenantRequired", "Tenant é obrigatório"));

        if (!_currentUser.UserId.HasValue)
            return Result.Failure(new Error("Cart.UserRequired", "Usuário é obrigatório"));

        var cart = await _cartRepository.GetByUserIdAsync(tenantId.Value, _currentUser.UserId.Value, cancellationToken);
        if (cart is null)
            return Result.Failure(new Error("Cart.NotFound", "Carrinho não encontrado"));

        var updateResult = cart.UpdateItemQuantity(command.ProductId, command.Quantity);
        if (updateResult.IsFailure)
            return Result.Failure(updateResult.Error);

        await _cartRepository.UpdateAsync(cart, cancellationToken);

        return Result.Success();
    }
}
