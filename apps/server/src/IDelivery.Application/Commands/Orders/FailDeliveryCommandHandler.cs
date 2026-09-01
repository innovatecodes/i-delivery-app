using IDelivery.Application.Abstractions.Authentication;
using IDelivery.Application.Abstractions.CQRS;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.Domain.Orders.Enums;
using IDelivery.SharedKernel.Common.Result;

namespace IDelivery.Application.Commands.Orders;

public sealed class FailDeliveryCommandHandler : ICommandHandler<FailDeliveryCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUser _currentUser;

    public FailDeliveryCommandHandler(
        IOrderRepository orderRepository,
        ITenantContext tenantContext,
        ICurrentUser currentUser)
    {
        _orderRepository = orderRepository;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(FailDeliveryCommand command, CancellationToken cancellationToken = default)
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

        var failResult = order.FailDelivery(_currentUser.UserId.Value, command.Reason, command.ReasonDetail);
        if (failResult.IsFailure)
            return Result.Failure(failResult.Error);

        await _orderRepository.UpdateAsync(order, cancellationToken);

        return Result.Success();
    }
}