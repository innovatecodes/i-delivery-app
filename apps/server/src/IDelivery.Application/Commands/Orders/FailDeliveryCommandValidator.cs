using FluentValidation;
using IDelivery.Domain.Orders.Enums;

namespace IDelivery.Application.Commands.Orders;

public sealed class FailDeliveryCommandValidator : AbstractValidator<FailDeliveryCommand>
{
    public FailDeliveryCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("ID do pedido é obrigatório");

        RuleFor(x => x.Reason)
            .IsInEnum().WithMessage("Motivo da falha inválido");

        When(x => x.Reason == DeliveryFailureReason.Other, () =>
        {
            RuleFor(x => x.ReasonDetail)
                .NotEmpty().WithMessage("Detalhe do motivo é obrigatório quando o motivo é 'Outro'")
                .MaximumLength(500).WithMessage("Detalhe do motivo deve ter no máximo 500 caracteres");
        });
    }
}