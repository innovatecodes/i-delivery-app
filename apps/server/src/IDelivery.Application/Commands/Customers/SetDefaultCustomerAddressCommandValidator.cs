using FluentValidation;

namespace IDelivery.Application.Commands.Customers;

public sealed class SetDefaultCustomerAddressCommandValidator : AbstractValidator<SetDefaultCustomerAddressCommand>
{
    public SetDefaultCustomerAddressCommandValidator()
    {
        RuleFor(x => x.AddressId)
            .NotEmpty().WithMessage("ID do endereço é obrigatório");
    }
}
