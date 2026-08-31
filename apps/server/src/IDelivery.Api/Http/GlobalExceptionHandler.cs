using System.Net;
using IDelivery.Application.Common.Exceptions;
using IDelivery.Domain.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace IDelivery.Api.Http;

/// <summary>
/// Handler global de exceções não tratadas.
/// Converte exceções de domínio e aplicação em respostas HTTP padronizadas.
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
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

        var problemDetails = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = GetTitle(statusCode),
            Detail = GetDetail(exception, statusCode),
            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode = (int)statusCode;

        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
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

            _ =>
                HttpStatusCode.InternalServerError
        };
    }

    private static string GetTitle(HttpStatusCode statusCode)
    {
        return statusCode switch
        {
            HttpStatusCode.BadRequest =>
                "Bad Request",

            HttpStatusCode.Unauthorized =>
                "Unauthorized",

            HttpStatusCode.Forbidden =>
                "Forbidden",

            HttpStatusCode.NotFound =>
                "Not Found",

            HttpStatusCode.Conflict =>
                "Conflict",

            _ =>
                "Internal Server Error"
        };
    }

    private static string GetDetail(
        Exception exception,
        HttpStatusCode statusCode)
    {
        if (statusCode != HttpStatusCode.InternalServerError)
            return exception.Message;

        return "Ocorreu um erro interno no servidor.";
    }
}