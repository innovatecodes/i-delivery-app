using IDelivery.Application.Abstractions.CQRS;
using IDelivery.Application.Commands.Auth;
using IDelivery.Application.Commands.Tenants;
using IDelivery.Application.Commands.Catalog;
using IDelivery.Application.Commands.Carts;
using IDelivery.Application.Queries.Tenants;
using IDelivery.Application.Queries.Catalog;
using IDelivery.Application.Queries.Carts;
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

        // Auth Commands
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

        // Catalog Commands
        services.AddScoped<
            ICommandHandler<CreateCategoryCommand, Guid>,
            CreateCategoryCommandHandler>();

        services.AddScoped<
            ICommandHandler<UpdateCategoryCommand>,
            UpdateCategoryCommandHandler>();

        services.AddScoped<
            ICommandHandler<DeleteCategoryCommand>,
            DeleteCategoryCommandHandler>();

        services.AddScoped<
            ICommandHandler<CreateProductCommand, Guid>,
            CreateProductCommandHandler>();

        services.AddScoped<
            ICommandHandler<UpdateProductCommand>,
            UpdateProductCommandHandler>();

        services.AddScoped<
            ICommandHandler<DeleteProductCommand>,
            DeleteProductCommandHandler>();

        // Tenant Queries
        services.AddScoped<
            IQueryHandler<GetTenantQuery, TenantResponse>,
            GetTenantQueryHandler>();

        services.AddScoped<
            IQueryHandler<GetTenantsQuery, PagedResult<TenantListItemResponse>>,
            GetTenantsQueryHandler>();

        // Catalog Queries
        services.AddScoped<
            ICommandHandler<GetCategoryQuery, CategoryResponse>,
            GetCategoryQueryHandler>();

        services.AddScoped<
            ICommandHandler<GetCategoriesByTenantQuery, IReadOnlyList<CategoryResponse>>,
            GetCategoriesByTenantQueryHandler>();

        services.AddScoped<
            ICommandHandler<GetProductQuery, ProductResponse>,
            GetProductQueryHandler>();

        services.AddScoped<
            ICommandHandler<GetProductsByTenantQuery, IReadOnlyList<ProductResponse>>,
            GetProductsByTenantQueryHandler>();

        services.AddScoped<
            ICommandHandler<GetProductsByCategoryQuery, IReadOnlyList<ProductResponse>>,
            GetProductsByCategoryQueryHandler>();

        // Cart Commands
        services.AddScoped<
            ICommandHandler<AddCartItemCommand>,
            AddCartItemCommandHandler>();

        services.AddScoped<
            ICommandHandler<RemoveCartItemCommand>,
            RemoveCartItemCommandHandler>();

        services.AddScoped<
            ICommandHandler<UpdateCartItemQuantityCommand>,
            UpdateCartItemQuantityCommandHandler>();

        services.AddScoped<
            ICommandHandler<ClearCartCommand>,
            ClearCartCommandHandler>();

        // Cart Queries
        services.AddScoped<
            IQueryHandler<GetCartQuery, CartResponse>,
            GetCartQueryHandler>();
    }

    private static void RegisterValidators(IServiceCollection services)
    {

        // O CreateTenantCommandValidator é utilizado apenas como referência para identificar o assembly da Application
        // O FluentValidation faz o scan de todo o assembly e registra automaticamente todos os validators encontrados, incluindo os validators de Auth, Tenants e outras funcionalidades
        services.AddValidatorsFromAssemblyContaining<CreateTenantCommandValidator>();

        //services.AddScoped<IValidator<CreateTenantCommand>, CreateTenantCommandValidator>();
    }
}