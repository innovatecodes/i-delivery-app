using IDelivery.Domain.Common.DomainEvents;

namespace IDelivery.Domain.Customers.Events;

public sealed class CustomerCreatedDomainEvent : DomainEvent
{
    public Guid CustomerId { get; }
    public Guid TenantId { get; }
    public Guid UserId { get; }
    public string Email { get; }

    public CustomerCreatedDomainEvent(Guid customerId, Guid tenantId, Guid userId, string email)
    {
        CustomerId = customerId;
        TenantId = tenantId;
        UserId = userId;
        Email = email;
    }
}
