using IDelivery.Application.Abstractions.Authentication;
using IDelivery.Application.Abstractions.Events;
using IDelivery.Application.Abstractions.Messaging;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.Application.Abstractions.Security;
using IDelivery.Application.Settings;
using IDelivery.Infrastructure.Authentication;
using IDelivery.Infrastructure.Events;
using IDelivery.Infrastructure.Messaging.Common;
using IDelivery.Infrastructure.Messaging.Email;
using IDelivery.Infrastructure.Persistence;
using IDelivery.Infrastructure.Persistence.Context;
using IDelivery.Infrastructure.Persistence.Repositories;
using IDelivery.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IDelivery.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Notification & Messaging
        services.AddTransient<INotificationService, NotificationService>();
        services.AddTransient<IClientUrlGenerator, ClientUrlGenerator>();

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
        services.AddScoped<IPaymentRepository, PaymentRepository>();

        // JwtOptions Authentication
        services.Configure<JwtOptions>(
            configuration.GetSection("Jwt"));

        services.AddScoped<IJwtTokenService, JwtTokenService>();

        // Security
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<ISecureTokenGenerator, SecureTokenGenerator>();
        services.AddScoped<ITokenHasher, TokenHasher>();

        // ClientSettings
        services.Configure<ClientSettings>(
            configuration.GetSection(ClientSettings.SectionName));

        // EmailOptions
        services.Configure<EmailOptions>(
            configuration.GetSection("Email"));

        // Email Service
        services.AddScoped<IEmailService, EmailService>();

        // Domain Events Dispatcher: Intercepta e despacha os eventos de domínio gerados pelas entidades, busca, automaticamente os Handlers na camada Application via Reflection
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        return services;
    }
}