namespace IDelivery.Application.Abstractions.Authentication;

public interface ICurrentUser
{
    Guid? UserId { get; }
    Guid? TenantId { get; }
    string[] Roles { get; }
    bool IsAuthenticated { get; }
}