using FluentValidation;
using IDelivery.Application.Commands.Catalog;

namespace IDelivery.Application.Commands.Catalog;

public sealed class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome é obrigatório")
            .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Descrição deve ter no máximo 500 caracteres");

        RuleFor(x => x.ImageUrl)
            .MaximumLength(2000).WithMessage("URL da imagem deve ter no máximo 2000 caracteres");

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Ordem de exibição deve ser maior ou igual a zero");
    }
}
