using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using IDelivery.Application;
using IDelivery.Infrastructure;

namespace IDelivery.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddApplication();
        builder.Services.AddInfrastructure(builder.Configuration);

        builder.Services.AddHealthChecks();

        var app = builder.Build();

        app.MapHealthChecks("/health");

        app.Run();
    }
}