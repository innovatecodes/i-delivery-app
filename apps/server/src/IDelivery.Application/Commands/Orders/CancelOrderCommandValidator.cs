using FluentValidation;

namespace IDelivery.Application.Commands.Orders;

public sealed class CancelOrderCommandValidator : AbstractValidator<CancelOrderCommand>
{
    public CancelOrderCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("ID do pedido é obrigatório");

        When(x => !string.IsNullOrWhiteSpace(x.CancelledBy), () =>
        {
            RuleFor(x => x.CancelledBy!)
                .MaximumLength(200).WithMessage("Identificação de quem cancelou deve ter no máximo 200 caracteres");
        });
    }
}