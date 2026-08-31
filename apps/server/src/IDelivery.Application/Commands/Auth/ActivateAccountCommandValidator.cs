using FluentValidation;

namespace IDelivery.Application.Commands.Auth;

public sealed class ActivateAccountCommandValidator : AbstractValidator<ActivateAccountCommand>
{
    public ActivateAccountCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token de ativação é obrigatório");
    }
}