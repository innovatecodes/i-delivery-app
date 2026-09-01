using FluentValidation;

namespace IDelivery.Application.Commands.Orders;

public sealed class StartPreparingOrderCommandValidator : AbstractValidator<StartPreparingOrderCommand>
{
    public StartPreparingOrderCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("ID do pedido é obrigatório");
    }
}