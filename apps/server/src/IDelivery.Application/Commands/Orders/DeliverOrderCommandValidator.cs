using FluentValidation;

namespace IDelivery.Application.Commands.Orders;

public sealed class DeliverOrderCommandValidator : AbstractValidator<DeliverOrderCommand>
{
    public DeliverOrderCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("ID do pedido é obrigatório");
    }
}