using IDelivery.Domain.Common.DomainEvents;

namespace IDelivery.Domain.Customers.Events;

public sealed class CustomerAddressRemovedDomainEvent : DomainEvent
{
    public Guid CustomerId { get; }
    public Guid TenantId { get; }
    public Guid AddressId { get; }

    public CustomerAddressRemovedDomainEvent(Guid customerId, Guid tenantId, Guid addressId)
    {
        CustomerId = customerId;
        TenantId = tenantId;
        AddressId = addressId;
    }
}
