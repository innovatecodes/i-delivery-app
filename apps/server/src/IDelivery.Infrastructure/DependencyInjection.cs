using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using IDelivery.Application.Abstractions.Events;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.Application.Abstractions.Security;
using IDelivery.Application.Abstractions.Services;
using IDelivery.Domain.Users.Events;
using IDelivery.Infrastructure.Email;
using IDelivery.Infrastructure.Events;
using IDelivery.Infrastructure.Events.Handlers;
using IDelivery.Infrastructure.Persistence;
using IDelivery.Infrastructure.Persistence.Context;
using IDelivery.Infrastructure.Persistence.Repositories;
using IDelivery.Infrastructure.Security;

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
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICartRepository, CartRepository>();

        // Security
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<ISecureTokenGenerator, SecureTokenGenerator>();
        services.AddScoped<ITokenHasher, TokenHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        
        var jwtOptions = new JwtOptions();
        configuration.GetSection("Jwt").Bind(jwtOptions);
        services.AddSingleton(jwtOptions);

        // Email
        services.AddScoped<IEmailService, EmailService>();
        
        var emailOptions = new EmailOptions();
        configuration.GetSection("Email").Bind(emailOptions);
        services.AddSingleton(emailOptions);

        // Domain Event Handlers
        services.AddScoped<IDomainEventHandler<UserRegisteredDomainEvent>, UserRegisteredDomainEventHandler>();

        return services;
    }
}