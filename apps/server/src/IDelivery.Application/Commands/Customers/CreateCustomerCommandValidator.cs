using FluentValidation;

namespace IDelivery.Application.Commands.Customers;

public sealed class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Usuário é obrigatório");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Nome completo é obrigatório")
            .MaximumLength(200).WithMessage("Nome completo deve ter no máximo 200 caracteres");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email é obrigatório")
            .EmailAddress().WithMessage("Email inválido")
            .MaximumLength(200).WithMessage("Email deve ter no máximo 200 caracteres");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("Telefone deve ter no máximo 20 caracteres");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Observações devem ter no máximo 1000 caracteres");
    }
}
