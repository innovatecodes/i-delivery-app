namespace IDelivery.Application.Abstractions.Authentication;

public interface ITenantContext
{
    Guid? TenantId { get; }
    bool HasTenant { get; }
}
