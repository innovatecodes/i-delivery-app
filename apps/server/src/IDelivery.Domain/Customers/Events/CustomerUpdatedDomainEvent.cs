using IDelivery.Domain.Common.DomainEvents;

namespace IDelivery.Domain.Customers.Events;

public sealed class CustomerUpdatedDomainEvent : DomainEvent
{
    public Guid CustomerId { get; }
    public Guid TenantId { get; }
    public Guid UserId { get; }

    public CustomerUpdatedDomainEvent(Guid customerId, Guid tenantId, Guid userId)
    {
        CustomerId = customerId;
        TenantId = tenantId;
        UserId = userId;
    }
}
