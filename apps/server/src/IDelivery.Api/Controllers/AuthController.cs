using Microsoft.AspNetCore.Mvc;
using IDelivery.Application.Abstractions.CQRS;
using IDelivery.Application.Commands.Auth;
using IDelivery.Application.Common.Models;
using IDelivery.SharedKernel.Common.Result;

namespace IDelivery.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly ICommandDispatcher _commandDispatcher;

    public AuthController(ICommandDispatcher commandDispatcher)
    {
        _commandDispatcher = commandDispatcher;
    }

    [HttpPost("register")]
    public async Task<ActionResult<Result<Guid>>> Register([FromBody] RegisterCommand command, CancellationToken cancellationToken)
    {
        var result = await _commandDispatcher.Dispatch<RegisterCommand, Guid>(command, cancellationToken);
        
        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error.Code, message = result.Error.Message });
        }

        return Ok(new { userId = result.Value, message = "Usuário registrado com sucesso. Verifique seu e-mail para ativação" });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResult>> Login([FromBody] LoginCommand command, CancellationToken cancellationToken)
    {
        var result = await _commandDispatcher.Dispatch<LoginCommand, AuthResult>(command, cancellationToken);
        
        if (result.IsFailure)
        {
            return Unauthorized(new { error = result.Error.Code, message = result.Error.Message });
        }

        return Ok(result.Value);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResult>> RefreshToken([FromBody] RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var result = await _commandDispatcher.Dispatch<RefreshTokenCommand, AuthResult>(command, cancellationToken);
        
        if (result.IsFailure)
        {
            return Unauthorized(new { error = result.Error.Code, message = result.Error.Message });
        }

        return Ok(result.Value);
    }

    [HttpPost("activate")]
    public async Task<ActionResult> ActivateAccount([FromBody] ActivateAccountCommand command, CancellationToken cancellationToken)
    {
        var result = await _commandDispatcher.Dispatch(command, cancellationToken);
        
        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error.Code, message = result.Error.Message });
        }

        return Ok(new { message = "Conta ativada com sucesso" });
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command, CancellationToken cancellationToken)
    {
        var result = await _commandDispatcher.Dispatch(command, cancellationToken);
        
        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error.Code, message = result.Error.Message });
        }

        return Ok(new { message = "Se o e-mail existir, você receberá instruções para redefinir a senha" });
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        var result = await _commandDispatcher.Dispatch(command, cancellationToken);
        
        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error.Code, message = result.Error.Message });
        }

        return Ok(new { message = "Senha redefinida com sucesso" });
    }

    [HttpGet("me")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public ActionResult<object> GetCurrentUser()
    {
        var userId = User.FindFirst("sub")?.Value;
        var tenantId = User.FindFirst("tenant_id")?.Value;
        var roles = User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToArray();
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? User.FindFirst("email")?.Value;

        return Ok(new
        {
            userId = Guid.TryParse(userId, out var uId) ? uId : (Guid?)null,
            tenantId = Guid.TryParse(tenantId, out var tId) ? tId : (Guid?)null,
            roles,
            email
        });
    }
}