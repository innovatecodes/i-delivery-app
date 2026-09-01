using FluentValidation;
using IDelivery.Domain.Delivery.Enums;

namespace IDelivery.Application.Commands.Delivery;

public sealed class UpdateDeliverySettingsCommandValidator : AbstractValidator<UpdateDeliverySettingsCommand>
{
    public UpdateDeliverySettingsCommandValidator()
    {
        RuleFor(x => x.FeeType)
            .IsInEnum().WithMessage("Tipo de taxa inválido");

        RuleFor(x => x.FixedFee)
            .GreaterThanOrEqualTo(0).WithMessage("Taxa fixa não pode ser negativa");

        When(x => x.FeeType == DeliveryFeeType.FreeAboveAmount, () =>
        {
            RuleFor(x => x.FreeAboveAmount)
                .NotNull().WithMessage("Valor para entrega grátis é obrigatório")
                .GreaterThan(0).WithMessage("Valor para entrega grátis deve ser maior que zero");
        });

        When(x => x.FeeType == DeliveryFeeType.PerDistance, () =>
        {
            RuleFor(x => x.FeePerKm)
                .NotNull().WithMessage("Taxa por km é obrigatória")
                .GreaterThan(0).WithMessage("Taxa por km deve ser maior que zero");
        });

        When(x => x.MinimumFee.HasValue, () =>
        {
            RuleFor(x => x.MinimumFee!.Value)
                .GreaterThanOrEqualTo(0).WithMessage("Taxa mínima não pode ser negativa");
        });

        When(x => x.MaximumFee.HasValue, () =>
        {
            RuleFor(x => x.MaximumFee!.Value)
                .GreaterThan(0).WithMessage("Taxa máxima deve ser maior que zero");
        });

        RuleFor(x => x)
            .Must(x => !x.MinimumFee.HasValue || !x.MaximumFee.HasValue || x.MaximumFee >= x.MinimumFee)
            .WithMessage("Taxa máxima deve ser maior ou igual à taxa mínima");
    }
}