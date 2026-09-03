using System.Reflection;
using IDelivery.Application.Abstractions.CQRS;
using IDelivery.Application.Abstractions.Events;
using IDelivery.Application.Dispatching;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace IDelivery.Application;

public static class DependencyInjection
{
    // Captura o assembly atual (camada Application) para servir de base para a varredura automática (Reflection)
    private static readonly Assembly ApplicationAssembly = Assembly.GetExecutingAssembly();

    /// <summary>
    /// Método de extensão principal para registrar todos os serviços da camada Application no DI.
    /// </summary>
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        // Registra o despachante de comandos (CommandDispatcher) que coordena a execução e o Unit of Work
        services.AddScoped<ICommandDispatcher, CommandDispatcher>();

        // Executa as varreduras automáticas (Reflection) para registrar handlers e validadores sem configuração manual
        RegisterCommandHandlers(services);
        RegisterQueryHandlers(services);
        RegisterDomainEventHandlers(services);
        RegisterValidators(services);

        return services;
    }

    /// <summary>
    /// Varre o assembly procurando por todas as classes que implementam ICommandHandler e as registra automaticamente no DI.
    /// </summary>
    private static void RegisterCommandHandlers(IServiceCollection services)
    {
        var handlerTypes = ApplicationAssembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false }) // Ignora classes abstratas e interfaces puras
            .SelectMany(t => t.GetInterfaces(), (type, iface) => (Type: type, Interface: iface)) // Associa cada classe às suas respectivas interfaces
            .Where(x => x.Interface.IsGenericType && // Filtra apenas interfaces genéricas...
                         (x.Interface.GetGenericTypeDefinition() == typeof(ICommandHandler<>) ||
                          x.Interface.GetGenericTypeDefinition() == typeof(ICommandHandler<,>))); // ...que correspondam aos contratos de Command Handler

        foreach (var (type, iface) in handlerTypes)
        {
            // Registra dinamicamente no container (Ex: ICommandHandler<CreateUserCommand> -> CreateUserCommandHandler)
            services.AddScoped(iface, type);
        }
    }

    /// <summary>
    /// Varre o assembly procurando por todas as classes que implementam IQueryHandler e as registra automaticamente no DI.
    /// </summary>
    private static void RegisterQueryHandlers(IServiceCollection services)
    {
        var handlerTypes = ApplicationAssembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false })
            .SelectMany(t => t.GetInterfaces(), (type, iface) => (Type: type, Interface: iface))
            .Where(x => x.Interface.IsGenericType &&
                         x.Interface.GetGenericTypeDefinition() == typeof(IQueryHandler<,>)); // Filtra contratos de Query Handler

        foreach (var (type, iface) in handlerTypes)
        {
            services.AddScoped(iface, type);
        }
    }

    /// <summary>
    /// Varre o assembly procurando por todas as classes que implementam IDomainEventHandler e as registra automaticamente no DI.
    /// </summary>
    private static void RegisterDomainEventHandlers(IServiceCollection services)
    {
        var handlerTypes = ApplicationAssembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false })
            .SelectMany(t => t.GetInterfaces(), (type, iface) => (Type: type, Interface: iface))
            .Where(x => x.Interface.IsGenericType &&
                         x.Interface.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>)); // Filtra contratos de Domain Event Handler

        foreach (var (type, iface) in handlerTypes)
        {
            services.AddScoped(iface, type);
        }
    }

    /// <summary>
    /// Utiliza a biblioteca FluentValidation para varrer o assembly e registrar automaticamente todos os validadores encontrados.
    /// </summary>
    private static void RegisterValidators(IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(ApplicationAssembly);
    }
}