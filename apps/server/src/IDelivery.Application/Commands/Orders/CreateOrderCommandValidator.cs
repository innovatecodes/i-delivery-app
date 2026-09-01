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

        RuleFor(x => x.DeliveryAddress)
            .NotEmpty().WithMessage("Endereço de entrega é obrigatório")
            .MaximumLength(500).WithMessage("Endereço de entrega deve ter no máximo 500 caracteres");

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