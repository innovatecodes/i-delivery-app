using IDelivery.Application.Abstractions.Authentication;
using IDelivery.Application.Abstractions.CQRS;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.SharedKernel.Common.Result;

namespace IDelivery.Application.Commands.Orders;

public sealed class CancelOrderCommandHandler : ICommandHandler<CancelOrderCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUser _currentUser;

    public CancelOrderCommandHandler(
        IOrderRepository orderRepository,
        ITenantContext tenantContext,
        ICurrentUser currentUser)
    {
        _orderRepository = orderRepository;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(CancelOrderCommand command, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantContext.TenantId;
        if (!tenantId.HasValue)
            return Result.Failure(new Error("Order.TenantRequired", "Tenant é obrigatório"));

        var order = await _orderRepository.GetByIdAsync(command.OrderId, cancellationToken);
        if (order is null)
            return Result.Failure(new Error("Order.NotFound", "Pedido não encontrado"));

        if (order.TenantId != tenantId.Value)
            return Result.Failure(new Error("Order.AccessDenied", "Acesso negado"));

        // Verifica se o usuário tem permissão para cancelar
        // TODO: Implementar verificação de role (TenantAdmin, Customer owner, etc.)
        var cancelledBy = command.CancelledBy ?? (_currentUser.UserId.HasValue ? _currentUser.UserId.Value.ToString() : "System");

        var cancelResult = order.Cancel(cancelledBy);
        if (cancelResult.IsFailure)
            return Result.Failure(cancelResult.Error);

        await _orderRepository.UpdateAsync(order, cancellationToken);

        return Result.Success();
    }
}