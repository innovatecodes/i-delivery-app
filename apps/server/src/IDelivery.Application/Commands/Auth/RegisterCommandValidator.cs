using FluentValidation;

namespace IDelivery.Application.Commands.Auth;

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email é obrigatório")
            .EmailAddress().WithMessage("Email inválido")
            .MaximumLength(255).WithMessage("Email deve ter no máximo 255 caracteres");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Senha é obrigatória")
            .MinimumLength(8).WithMessage("Senha deve ter no mínimo 8 caracteres")
            .MaximumLength(100).WithMessage("Senha deve ter no máximo 100 caracteres")
            .Matches("[A-Z]").WithMessage("Senha deve conter pelo menos uma letra maiúscula")
            .Matches("[a-z]").WithMessage("Senha deve conter pelo menos uma letra minúscula")
            .Matches("[0-9]").WithMessage("Senha deve conter pelo menos um número")
            .Matches("[^a-zA-Z0-9]").WithMessage("Senha deve conter pelo menos um caractere especial");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Nome completo é obrigatório")
            .MaximumLength(200).WithMessage("Nome completo deve ter no máximo 200 caracteres");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("Telefone deve ter no máximo 20 caracteres")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
    }
}
