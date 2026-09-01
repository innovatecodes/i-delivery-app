using FluentValidation;

namespace IDelivery.Application.Commands.Carts;

public sealed class UpdateCartItemQuantityCommandValidator : AbstractValidator<UpdateCartItemQuantityCommand>
{
    public UpdateCartItemQuantityCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Produto é obrigatório");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantidade deve ser maior que zero");
    }
}
