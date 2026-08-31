using IDelivery.Application.Abstractions.CQRS;
using IDelivery.Application.Common.Models;

namespace IDelivery.Application.Commands.Auth;

public sealed record LoginCommand(
    string Email,
    string Password) : ICommand<AuthResult>;

public sealed record AuthResult(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt,
    Guid UserId,
    string Email,
    string FullName,
    string[] Roles,
    Guid? TenantId);