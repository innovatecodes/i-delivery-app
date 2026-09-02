using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using IDelivery.Application.Abstractions.Authentication;
using IDelivery.Application.Abstractions.Events;
using IDelivery.Application.Abstractions.Messaging;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.Application.Abstractions.Security;
using IDelivery.Domain.Users.Events;
using IDelivery.Infrastructure.Authentication;
using IDelivery.Infrastructure.Events;
using IDelivery.Infrastructure.Events.Handlers;
using IDelivery.Infrastructure.Messaging.Email;
using IDelivery.Infrastructure.Persistence;
using IDelivery.Infrastructure.Persistence.Context;
using IDelivery.Infrastructure.Persistence.Repositories;
using IDelivery.Infrastructure.Security;

namespace IDelivery.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Persistence
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IDeliverySettingsRepository, DeliverySettingsRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();

        // Authentication

        // Abordagem manual — mantida apenas como referência
        // var jwtOptions = new JwtOptions();
        // configuration.GetSection("Jwt").Bind(jwtOptions);
        // services.AddSingleton(jwtOptions);

        // Abordagem atual — Options Pattern
        services.Configure<JwtOptions>(
            configuration.GetSection("Jwt"));

        services.AddScoped<IJwtTokenService, JwtTokenService>();

        // Security
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<ISecureTokenGenerator, SecureTokenGenerator>();
        services.AddScoped<ITokenHasher, TokenHasher>();

        // Email
        services.Configure<EmailOptions>(
            configuration.GetSection("Email"));

        services.AddScoped<IEmailService, EmailService>();

        // Domain Events
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        services.AddScoped<
            IDomainEventHandler<UserRegisteredDomainEvent>,
            UserRegisteredDomainEventHandler>();

        return services;
    }
}