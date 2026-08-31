using IDelivery.Application.Abstractions.CQRS;
using IDelivery.Application.Commands.Tenants;
using IDelivery.Application.Queries.Tenants;
using IDelivery.Application.Common.Models;
using Microsoft.Extensions.DependencyInjection;

namespace IDelivery.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        RegisterHandlersManually(services);

        return services;
    }

    /*
    private static void RegisterHandlersByReflection(
        IServiceCollection services)
    {
        // Obtém o Assembly da camada Application.
        var assembly = typeof(DependencyInjection).Assembly;

        // Localiza todas as classes concretas e não abstratas
        // que possuem o sufixo "Handler".
        var handlers = assembly
            .GetTypes()
            .Where(type =>
                type is { IsClass: true, IsAbstract: false } &&
                type.Name.EndsWith("Handler"));

        // Registra cada CommandHandler e QueryHandler encontrado
        // automaticamente na Dependency Injection.
        foreach (var handler in handlers)
        {
            var interfaces = handler.GetInterfaces();

            foreach (var service in interfaces)
            {
                // Verifica se a interface representa um CommandHandler
                // ou QueryHandler antes de registrá-la.
                if (service.Name.StartsWith("ICommandHandler") ||
                    service.Name.StartsWith("IQueryHandler"))
                {
                    services.AddScoped(service, handler);
                }
            }
        }
    }
    */

    // Registra manualmente os CommandHandlers e QueryHandlers
    // utilizados pela camada Application.
    private static void RegisterHandlersManually(
        IServiceCollection services)
    {
        // Commands

        // Tenant Commands
        services.AddScoped<
            ICommandHandler<CreateTenantCommand, Guid>,
            CreateTenantCommandHandler>();

        services.AddScoped<
            ICommandHandler<ActivateTenantCommand>,
            ActivateTenantCommandHandler>();

        services.AddScoped<
            ICommandHandler<BlockTenantCommand>,
            BlockTenantCommandHandler>();

        services.AddScoped<
            ICommandHandler<DeleteTenantCommand>,
            DeleteTenantCommandHandler>();

        services.AddScoped<
            ICommandHandler<UpdateTenantCommand>,
            UpdateTenantCommandHandler>();

        // Queries

        // Tenant Queries
        services.AddScoped<
            IQueryHandler<GetTenantQuery, TenantResponse>,
            GetTenantQueryHandler>();

        services.AddScoped<
            IQueryHandler<GetTenantsQuery, PagedResult<TenantListItemResponse>>,
            GetTenantsQueryHandler>();
    }
}