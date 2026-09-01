using FluentValidation;

namespace IDelivery.Application.Commands.Carts;

public sealed class AddCartItemCommandValidator : AbstractValidator<AddCartItemCommand>
{
    public AddCartItemCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Produto é obrigatório");

        RuleFor(x => x.ProductName)
            .NotEmpty().WithMessage("Nome do produto é obrigatório")
            .MaximumLength(200).WithMessage("Nome do produto deve ter no máximo 200 caracteres");

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Preço não pode ser negativo");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Moeda é obrigatória")
            .Length(3).WithMessage("Moeda deve ter 3 caracteres");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantidade deve ser maior que zero");
    }
}
