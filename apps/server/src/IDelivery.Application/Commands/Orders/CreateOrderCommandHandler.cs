using IDelivery.Application.Abstractions.Authentication;
using IDelivery.Application.Abstractions.CQRS;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.Domain.Customers.Entities;
using IDelivery.Domain.Delivery.Entities;
using IDelivery.Domain.Orders.Entities;
using IDelivery.Domain.Orders.Enums;
using IDelivery.SharedKernel.Common.Result;

namespace IDelivery.Application.Commands.Orders;

public sealed class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, Guid>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICartRepository _cartRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IDeliverySettingsRepository _deliverySettingsRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUser _currentUser;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        ICartRepository cartRepository,
        ICustomerRepository customerRepository,
        IDeliverySettingsRepository deliverySettingsRepository,
        ITenantContext tenantContext,
        ICurrentUser currentUser)
    {
        _orderRepository = orderRepository;
        _cartRepository = cartRepository;
        _customerRepository = customerRepository;
        _deliverySettingsRepository = deliverySettingsRepository;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(CreateOrderCommand command, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantContext.TenantId;
        if (!tenantId.HasValue)
            return Result.Failure<Guid>(new Error("Order.TenantRequired", "Tenant é obrigatório"));

        if (!_currentUser.UserId.HasValue)
            return Result.Failure<Guid>(new Error("Order.UserRequired", "Usuário é obrigatório"));

        var customer = await _customerRepository.GetByUserIdAsync(tenantId.Value, _currentUser.UserId.Value, cancellationToken);
        if (customer is null)
            return Result.Failure<Guid>(new Error("Order.CustomerNotFound", "Perfil do cliente não encontrado. Complete seu cadastro primeiro."));

        var cart = await _cartRepository.GetByUserIdAsync(tenantId.Value, _currentUser.UserId.Value, cancellationToken);
        if (cart is null || cart.Items.Count == 0)
            return Result.Failure<Guid>(new Error("Order.CartEmpty", "Carrinho está vazio"));

        var items = new List<OrderItem>();
        foreach (var cartItem in cart.Items)
        {
            var itemResult = OrderItem.Create(
                Guid.Empty,
                cartItem.ProductId,
                cartItem.ProductName,
                cartItem.UnitPrice,
                cartItem.Currency,
                cartItem.Quantity);

            if (itemResult.IsFailure)
                return Result.Failure<Guid>(itemResult.Error);

            items.Add(itemResult.Value);
        }

        var deliveryFee = command.DeliveryFee;
        if (deliveryFee == 0)
        {
            var settings = await _deliverySettingsRepository.GetByTenantIdAsync(tenantId.Value, cancellationToken);
            if (settings is not null)
            {
                deliveryFee = settings.CalculateFee(items.Sum(i => i.GetSubtotal()), command.DeliveryDistanceKm);
            }
        }

        var orderResult = Order.Create(
            tenantId.Value,
            customer.Id,
            items,
            deliveryFee,
            command.Currency,
            command.DeliveryAddress,
            command.DeliveryDistanceKm);

        if (orderResult.IsFailure)
            return Result.Failure<Guid>(orderResult.Error);

        var order = orderResult.Value;
        await _orderRepository.AddAsync(order, cancellationToken);

        await _cartRepository.UpdateAsync(cart.Clear().IsSuccess ? cart : cart, cancellationToken);

        return Result.Success(order.Id);
    }
}