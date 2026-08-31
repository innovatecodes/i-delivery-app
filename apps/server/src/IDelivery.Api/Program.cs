//using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using IDelivery.Application;
using IDelivery.Application.Abstractions.Authentication;
using IDelivery.Infrastructure;
using IDelivery.Api.Http;
using IDelivery.Api.Services;

namespace IDelivery.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Controllers + Configuração JSON.
        builder.Services.AddControllers();

        // HttpContextAccessor para ICurrentUser
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ICurrentUser, CurrentUserService>();

        // Exception Handler Global - registra o IExceptionHandler.
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();

        // JWT Authentication
        var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>();
        if (jwtOptions is not null)
        {
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtOptions.Issuer,
                        ValidAudience = jwtOptions.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
                        ClockSkew = TimeSpan.Zero
                    };
                });
        }

        builder.Services.AddAuthorization();

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

        // Authentication & Authorization
        app.UseAuthentication();
        app.UseAuthorization();

        // Mapeia o endpoint /health para Health Checks.
        app.MapHealthChecks("/health");

        app.MapControllers();

        app.Run();
    }

    private sealed class JwtOptions
    {
        public string SecretKey { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int AccessTokenExpirationMinutes { get; set; } = 15;
        public int RefreshTokenExpirationDays { get; set; } = 7;
    }
}