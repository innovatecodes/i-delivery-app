using FluentValidation;
using IDelivery.Application.Commands.Catalog;

namespace IDelivery.Application.Commands.Catalog;

public sealed class DeleteCategoryCommandValidator : AbstractValidator<DeleteCategoryCommand>
{
    public DeleteCategoryCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id é obrigatório");
    }
}
