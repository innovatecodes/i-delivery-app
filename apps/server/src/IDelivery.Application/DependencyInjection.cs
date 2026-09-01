using IDelivery.Application.Abstractions.CQRS;
using IDelivery.Application.Commands.Auth;
using IDelivery.Application.Commands.Tenants;
using IDelivery.Application.Queries.Tenants;
using IDelivery.Application.Common.Models;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace IDelivery.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        // Registra automaticamente todos os validators encontrados no assembly IDelivery.Application
        RegisterValidators(services);

        // Registra manualmente os CommandHandlers e QueryHandlers utilizados pela camada Application
        RegisterHandlersManually(services);
        
        return services;
    }

    private static void RegisterHandlersManually(
        IServiceCollection services)
    {
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

        services.AddScoped<
            ICommandHandler<RegisterCommand, Guid>,
            RegisterCommandHandler>();

        services.AddScoped<
            ICommandHandler<LoginCommand, AuthResult>,
            LoginCommandHandler>();

        services.AddScoped<
            ICommandHandler<RefreshTokenCommand, AuthResult>,
            RefreshTokenCommandHandler>();

        services.AddScoped<
            ICommandHandler<ActivateAccountCommand>,
            ActivateAccountCommandHandler>();

        services.AddScoped<
            ICommandHandler<ForgotPasswordCommand>,
            ForgotPasswordCommandHandler>();

        services.AddScoped<
            ICommandHandler<ResetPasswordCommand>,
            ResetPasswordCommandHandler>();

        // Tenant Queries
        services.AddScoped<
            IQueryHandler<GetTenantQuery, TenantResponse>,
            GetTenantQueryHandler>();

        services.AddScoped<
            IQueryHandler<GetTenantsQuery, PagedResult<TenantListItemResponse>>,
            GetTenantsQueryHandler>();
    }

    private static void RegisterValidators(IServiceCollection services)
    {

        // O CreateTenantCommandValidator é utilizado apenas como referência para identificar o assembly da Application
        // O FluentValidation faz o scan de todo o assembly e registra automaticamente todos os validators encontrados, incluindo os validators de Auth, Tenants e outras funcionalidades
        services.AddValidatorsFromAssemblyContaining<CreateTenantCommandValidator>();

        //services.AddScoped<IValidator<CreateTenantCommand>, CreateTenantCommandValidator>();
    }
}