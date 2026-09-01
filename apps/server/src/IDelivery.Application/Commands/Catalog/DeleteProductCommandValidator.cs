using FluentValidation;
using IDelivery.Application.Commands.Catalog;

namespace IDelivery.Application.Commands.Catalog;

public sealed class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
{
    public DeleteProductCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id é obrigatório");
    }
}
