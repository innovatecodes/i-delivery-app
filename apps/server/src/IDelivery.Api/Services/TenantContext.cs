using IDelivery.Application.Abstractions.Authentication;

namespace IDelivery.Api.Services;

public sealed class TenantContext : ITenantContext
{
    private readonly ICurrentUser _currentUser;

    public TenantContext(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    public Guid? TenantId => _currentUser.TenantId;

    public bool HasTenant => _currentUser.TenantId.HasValue;
}
