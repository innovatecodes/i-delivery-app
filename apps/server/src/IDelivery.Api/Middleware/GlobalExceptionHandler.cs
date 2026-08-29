using System.Net;
using IDelivery.Api.Common;
using IDelivery.Application.Common.Exceptions;
using IDelivery.Domain.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace IDelivery.Api.Middleware;

/// <summary>
/// Handler global de exceções não tratadas.
/// Converte exceções de domínio em respostas HTTP padronizadas.
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IWebHostEnvironment _environment;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        IWebHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(
            exception,
            "Exceção não tratada: {Message}",
            exception.Message);

        var statusCode = GetStatusCode(exception);
        var message = GetMessage(exception, statusCode);

        var response = new
        {
            StatusCode = (int)statusCode,
            Message = message,
            Errors = Array.Empty<string>(),
            StackTrace = _environment.IsDevelopment()
                ? exception.StackTrace
                : null
        };

        httpContext.Response.StatusCode = (int)statusCode;
        httpContext.Response.ContentType = "application/json";

        await httpContext.Response.WriteAsJsonAsync(
            response,
            cancellationToken);

        return true;
    }

    private static HttpStatusCode GetStatusCode(Exception exception)
    {
        return exception switch
        {
            DomainException =>
                HttpStatusCode.BadRequest,

            ValidationException =>
                HttpStatusCode.BadRequest,

            BadRequestException =>
                HttpStatusCode.BadRequest,

            NotFoundException =>
                HttpStatusCode.NotFound,

            UnauthorizedException =>
                HttpStatusCode.Unauthorized,

            ForbiddenException =>
                HttpStatusCode.Forbidden,

            ConflictException =>
                HttpStatusCode.Conflict,

            ArgumentOutOfRangeException =>
                HttpStatusCode.BadRequest,

            DivideByZeroException =>
                HttpStatusCode.InternalServerError,

            _ =>
                HttpStatusCode.InternalServerError
        };
    }

    private static string GetMessage(
        Exception exception,
        HttpStatusCode statusCode)
    {
        if (statusCode != HttpStatusCode.InternalServerError &&
            !string.IsNullOrWhiteSpace(exception.Message))
        {
            return exception.Message;
        }

        return HttpStatusCodeMessages.Get(statusCode);
    }
}