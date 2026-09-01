using FluentValidation;

namespace IDelivery.Application.Commands.Customers;

public sealed class AddCustomerAddressCommandValidator : AbstractValidator<AddCustomerAddressCommand>
{
    public AddCustomerAddressCommandValidator()
    {
        RuleFor(x => x.Label)
            .NotEmpty().WithMessage("Rótulo do endereço é obrigatório")
            .MaximumLength(50).WithMessage("Rótulo do endereço deve ter no máximo 50 caracteres");

        RuleFor(x => x.Street)
            .NotEmpty().WithMessage("Rua é obrigatória")
            .MaximumLength(200).WithMessage("Rua deve ter no máximo 200 caracteres");

        RuleFor(x => x.Number)
            .NotEmpty().WithMessage("Número é obrigatório")
            .MaximumLength(20).WithMessage("Número deve ter no máximo 20 caracteres");

        RuleFor(x => x.Neighborhood)
            .NotEmpty().WithMessage("Bairro é obrigatório")
            .MaximumLength(100).WithMessage("Bairro deve ter no máximo 100 caracteres");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("Cidade é obrigatória")
            .MaximumLength(100).WithMessage("Cidade deve ter no máximo 100 caracteres");

        RuleFor(x => x.State)
            .NotEmpty().WithMessage("Estado é obrigatório")
            .MaximumLength(2).WithMessage("Estado deve ter no máximo 2 caracteres");

        RuleFor(x => x.ZipCode)
            .NotEmpty().WithMessage("CEP é obrigatório")
            .MaximumLength(10).WithMessage("CEP deve ter no máximo 10 caracteres");

        RuleFor(x => x.Reference)
            .MaximumLength(200).WithMessage("Referência deve ter no máximo 200 caracteres");
    }
}
