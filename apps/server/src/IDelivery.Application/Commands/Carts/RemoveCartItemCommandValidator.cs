using FluentValidation;

namespace IDelivery.Application.Commands.Carts;

public sealed class RemoveCartItemCommandValidator : AbstractValidator<RemoveCartItemCommand>
{
    public RemoveCartItemCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Produto é obrigatório");
    }
}
