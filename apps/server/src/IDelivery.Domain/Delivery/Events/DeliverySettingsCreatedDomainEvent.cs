using IDelivery.Domain.Common.DomainEvents;
using IDelivery.Domain.Delivery.Enums;

namespace IDelivery.Domain.Delivery.Events;

public sealed class DeliverySettingsCreatedDomainEvent : DomainEvent
{
    public Guid DeliverySettingsId { get; }
    public Guid TenantId { get; }
    public DeliveryFeeType FeeType { get; }

    public DeliverySettingsCreatedDomainEvent(Guid deliverySettingsId, Guid tenantId, DeliveryFeeType feeType)
    {
        DeliverySettingsId = deliverySettingsId;
        TenantId = tenantId;
        FeeType = feeType;
    }
}