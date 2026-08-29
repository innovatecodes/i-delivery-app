using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using IDelivery.Application;
using IDelivery.Infrastructure;
using IDelivery.Api.Middleware;

namespace IDelivery.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Controllers + Configuração JSON.
        builder.Services.AddControllers();

        // Exception Handler Global - registra o IExceptionHandler.
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();

        // Camada de Aplicação.
        builder.Services.AddApplication();

        // Camada de Infraestrutura.
        builder.Services.AddInfrastructure(builder.Configuration);

        // Health Checks.
        builder.Services.AddHealthChecks();

        var app = builder.Build();

        // ===================================
        // PIPELINE HTTP (Middleware Pipeline)
        // ===================================

        // Exception Handler - deve ser um dos primeiros middlewares.
        app.UseExceptionHandler();

        // Mapeia o endpoint /health para Health Checks.
        app.MapHealthChecks("/health");

        app.MapControllers();

        app.Run();
    }
}