using FluentValidation;

namespace IDelivery.Application.Commands.Auth;

public sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token de reset é obrigatório");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Nova senha é obrigatória")
            .MinimumLength(8).WithMessage("Senha deve ter no mínimo 8 caracteres")
            .MaximumLength(100).WithMessage("Senha deve ter no máximo 100 caracteres")
            .Matches("[A-Z]").WithMessage("Senha deve conter pelo menos uma letra maiúscula")
            .Matches("[a-z]").WithMessage("Senha deve conter pelo menos uma letra minúscula")
            .Matches("[0-9]").WithMessage("Senha deve conter pelo menos um número")
            .Matches("[^a-zA-Z0-9]").WithMessage("Senha deve conter pelo menos um caractere especial");
    }
}