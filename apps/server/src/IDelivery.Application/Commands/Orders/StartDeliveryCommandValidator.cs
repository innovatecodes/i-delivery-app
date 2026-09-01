using FluentValidation;

namespace IDelivery.Application.Commands.Orders;

public sealed class StartDeliveryCommandValidator : AbstractValidator<StartDeliveryCommand>
{
    public StartDeliveryCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("ID do pedido é obrigatório");

        RuleFor(x => x.DeliveryDriverId)
            .NotEmpty().WithMessage("ID do entregador é obrigatório");
    }
}