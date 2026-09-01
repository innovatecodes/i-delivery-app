using IDelivery.Application.Abstractions.Authentication;
using IDelivery.Application.Abstractions.CQRS;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.SharedKernel.Common.Result;

namespace IDelivery.Application.Commands.Orders;

public sealed class StartDeliveryCommandHandler : ICommandHandler<StartDeliveryCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ITenantContext _tenantContext;

    public StartDeliveryCommandHandler(
        IOrderRepository orderRepository,
        ITenantContext tenantContext)
    {
        _orderRepository = orderRepository;
        _tenantContext = tenantContext;
    }

    public async Task<Result> Handle(StartDeliveryCommand command, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantContext.TenantId;
        if (!tenantId.HasValue)
            return Result.Failure(new Error("Order.TenantRequired", "Tenant é obrigatório"));

        var order = await _orderRepository.GetByIdAsync(command.OrderId, cancellationToken);
        if (order is null)
            return Result.Failure(new Error("Order.NotFound", "Pedido não encontrado"));

        if (order.TenantId != tenantId.Value)
            return Result.Failure(new Error("Order.AccessDenied", "Acesso negado"));

        var startResult = order.StartDelivery(command.DeliveryDriverId);
        if (startResult.IsFailure)
            return Result.Failure(startResult.Error);

        await _orderRepository.UpdateAsync(order, cancellationToken);

        return Result.Success();
    }
}