using FluentValidation;

namespace IDelivery.Application.Commands.Orders;

public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Pedido deve ter pelo menos um item");

        RuleForEach(x => x.Items).SetValidator(new CreateOrderItemDtoValidator());

        RuleFor(x => x.DeliveryFee)
            .GreaterThanOrEqualTo(0).WithMessage("Taxa de entrega não pode ser negativa");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Moeda é obrigatória")
            .Length(3).WithMessage("Moeda deve ter 3 caracteres");

        RuleFor(x => x.DeliveryStreet)
            .NotEmpty().WithMessage("Rua é obrigatória")
            .MaximumLength(200).WithMessage("Rua deve ter no máximo 200 caracteres");

        RuleFor(x => x.DeliveryNumber)
            .NotEmpty().WithMessage("Número é obrigatório")
            .MaximumLength(20).WithMessage("Número deve ter no máximo 20 caracteres");

        RuleFor(x => x.DeliveryNeighborhood)
            .NotEmpty().WithMessage("Bairro é obrigatório")
            .MaximumLength(100).WithMessage("Bairro deve ter no máximo 100 caracteres");

        RuleFor(x => x.DeliveryCity)
            .NotEmpty().WithMessage("Cidade é obrigatória")
            .MaximumLength(100).WithMessage("Cidade deve ter no máximo 100 caracteres");

        RuleFor(x => x.DeliveryState)
            .NotEmpty().WithMessage("Estado é obrigatório")
            .MaximumLength(2).WithMessage("Estado deve ter no máximo 2 caracteres");

        RuleFor(x => x.DeliveryZipCode)
            .NotEmpty().WithMessage("CEP é obrigatório")
            .MaximumLength(10).WithMessage("CEP deve ter no máximo 10 caracteres");

        When(x => x.DeliveryDistanceKm.HasValue, () =>
        {
            RuleFor(x => x.DeliveryDistanceKm!.Value)
                .GreaterThan(0).WithMessage("Distância de entrega deve ser maior que zero");
        });
    }
}

public sealed class CreateOrderItemDtoValidator : AbstractValidator<CreateOrderItemDto>
{
    public CreateOrderItemDtoValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Produto é obrigatório");

        RuleFor(x => x.ProductName)
            .NotEmpty().WithMessage("Nome do produto é obrigatório")
            .MaximumLength(200).WithMessage("Nome do produto deve ter no máximo 200 caracteres");

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Preço não pode ser negativo");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Moeda é obrigatória")
            .Length(3).WithMessage("Moeda deve ter 3 caracteres");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantidade deve ser maior que zero");
    }
}
