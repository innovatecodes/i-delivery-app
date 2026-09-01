using FluentValidation;
using IDelivery.Application.Commands.Tenants;

namespace IDelivery.Application.Commands.Tenants;

public sealed class UpdateTenantCommandValidator : AbstractValidator<UpdateTenantCommand>
{
    public UpdateTenantCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id é obrigatório");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome é obrigatório")
            .MaximumLength(200).WithMessage("Nome deve ter no máximo 200 caracteres");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Descrição deve ter no máximo 1000 caracteres");

        RuleFor(x => x.LogoUrl)
            .Must(uri => string.IsNullOrEmpty(uri) || Uri.IsWellFormedUriString(uri, UriKind.Absolute))
            .WithMessage("LogoUrl deve ser uma URL válida");
    }
}