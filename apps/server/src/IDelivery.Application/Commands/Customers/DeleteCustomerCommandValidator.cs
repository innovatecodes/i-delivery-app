using FluentValidation;

namespace IDelivery.Application.Commands.Customers;

public sealed class DeleteCustomerCommandValidator : AbstractValidator<DeleteCustomerCommand>
{
    public DeleteCustomerCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID do cliente é obrigatório");
    }
}
