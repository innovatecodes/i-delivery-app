using IDelivery.Application.Abstractions.CQRS;
using IDelivery.Application.Common.Models;

namespace IDelivery.Application.Commands.Auth;

public sealed record RefreshTokenCommand(
    string RefreshToken) : ICommand<AuthResult>;