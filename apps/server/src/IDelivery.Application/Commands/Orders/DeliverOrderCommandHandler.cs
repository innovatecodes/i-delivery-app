using IDelivery.Application.Abstractions.Authentication;
using IDelivery.Application.Abstractions.CQRS;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.SharedKernel.Common.Result;

namespace IDelivery.Application.Commands.Orders;

public sealed class DeliverOrderCommandHandler : ICommandHandler<DeliverOrderCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUser _currentUser;

    public DeliverOrderCommandHandler(
        IOrderRepository orderRepository,
        ITenantContext tenantContext,
        ICurrentUser currentUser)
    {
        _orderRepository = orderRepository;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(DeliverOrderCommand command, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantContext.TenantId;
        if (!tenantId.HasValue)
            return Result.Failure(new Error("Order.TenantRequired", "Tenant é obrigatório"));

        if (!_currentUser.UserId.HasValue)
            return Result.Failure(new Error("Order.UserRequired", "Usuário é obrigatório"));

        var order = await _orderRepository.GetByIdAsync(command.OrderId, cancellationToken);
        if (order is null)
            return Result.Failure(new Error("Order.NotFound", "Pedido não encontrado"));

        if (order.TenantId != tenantId.Value)
            return Result.Failure(new Error("Order.AccessDenied", "Acesso negado"));

        var deliverResult = order.Deliver(_currentUser.UserId.Value);
        if (deliverResult.IsFailure)
            return Result.Failure(deliverResult.Error);

        await _orderRepository.UpdateAsync(order, cancellationToken);

        return Result.Success();
    }
}