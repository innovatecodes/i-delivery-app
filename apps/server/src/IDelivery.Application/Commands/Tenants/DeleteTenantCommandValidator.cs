using FluentValidation;
using IDelivery.Application.Commands.Tenants;

namespace IDelivery.Application.Commands.Tenants;

public sealed class DeleteTenantCommandValidator : AbstractValidator<DeleteTenantCommand>
{
    public DeleteTenantCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id é obrigatório.");
    }
}