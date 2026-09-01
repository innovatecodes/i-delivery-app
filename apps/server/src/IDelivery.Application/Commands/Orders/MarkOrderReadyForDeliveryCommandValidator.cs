using FluentValidation;

namespace IDelivery.Application.Commands.Orders;

public sealed class MarkOrderReadyForDeliveryCommandValidator : AbstractValidator<MarkOrderReadyForDeliveryCommand>
{
    public MarkOrderReadyForDeliveryCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("ID do pedido é obrigatório");
    }
}