using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using IDelivery.Application.Abstractions.Events;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.Infrastructure.Events;
using IDelivery.Infrastructure.Persistence;
using IDelivery.Infrastructure.Persistence.Context;
using IDelivery.Infrastructure.Persistence.Repositories;

namespace IDelivery.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<ITenantRepository, TenantRepository>();

        return services;
    }
}