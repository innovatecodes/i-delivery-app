using FluentValidation;

namespace IDelivery.Application.Commands.Tenants;

public sealed class CreateTenantCommandValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(200).WithMessage("Nome deve ter no máximo 200 caracteres.");

        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Slug é obrigatório.")
            .MaximumLength(100).WithMessage("Slug deve ter no máximo 100 caracteres.")
            .Matches("^[a-z0-9-]+$").WithMessage("Slug deve conter apenas letras minúsculas, números e hífens.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Descrição deve ter no máximo 1000 caracteres.");

        RuleFor(x => x.LogoUrl)
            .Must(uri => string.IsNullOrEmpty(uri) || Uri.IsWellFormedUriString(uri, UriKind.Absolute))
            .WithMessage("LogoUrl deve ser uma URL válida.");

        RuleFor(x => x.Email)
            .Must(email => email is null || email.Value.Contains("@"))
            .WithMessage("Email deve ser válido.");
    }
}