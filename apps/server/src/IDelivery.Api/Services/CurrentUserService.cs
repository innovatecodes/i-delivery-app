using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using IDelivery.Application.Abstractions.Authentication;

namespace IDelivery.Api.Services;

public sealed class CurrentUserService : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value
                ?? _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }

    public Guid? TenantId
    {
        get
        {
            var tenantIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("tenant_id")?.Value;
            return Guid.TryParse(tenantIdClaim, out var tenantId) ? tenantId : null;
        }
    }

    public string[] Roles
    {
        get
        {
            return _httpContextAccessor.HttpContext?.User?.FindAll(ClaimTypes.Role)
                .Select(c => c.Value)
                .ToArray() ?? Array.Empty<string>();
        }
    }

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}