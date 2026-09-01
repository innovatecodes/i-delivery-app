using IDelivery.Domain.Common.DomainEvents;

namespace IDelivery.Domain.Customers.Events;

public sealed class CustomerAddressAddedDomainEvent : DomainEvent
{
    public Guid CustomerId { get; }
    public Guid TenantId { get; }
    public Guid AddressId { get; }

    public CustomerAddressAddedDomainEvent(Guid customerId, Guid tenantId, Guid addressId)
    {
        CustomerId = customerId;
        TenantId = tenantId;
        AddressId = addressId;
    }
}
