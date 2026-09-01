using IDelivery.Application.Abstractions.Persistence;
using IDelivery.Application.Abstractions.Security;
using IDelivery.SharedKernel.Common.Result;
using IDelivery.Domain.Tenants.Entities;
using IDelivery.Application.Abstractions.CQRS;

namespace IDelivery.Application.Commands.Tenants;

public sealed class CreateTenantCommandHandler : ICommandHandler<CreateTenantCommand, Guid>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ISecureTokenGenerator _tokenGenerator;

    public CreateTenantCommandHandler( 
        ITenantRepository tenantRepository,
        IPasswordHasher passwordHasher,
        ISecureTokenGenerator tokenGenerator)
    {
        _tenantRepository = tenantRepository;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<Result<Guid>> Handle(CreateTenantCommand command, CancellationToken cancellationToken = default)
    {
        if (await _tenantRepository.ExistsBySlugAsync(command.Slug, cancellationToken))
        {
            return Result.Failure<Guid>(new Error("Tenant.SlugAlreadyExists", "Slug já está em uso"));
        }

        var tenantResult = Tenant.Create(
            command.Name,
            command.Slug,
            command.Description,
            command.LogoUrl,
            command.Address,
            command.Email,
            command.Phone,
            command.WhatsApp);

        if (tenantResult.IsFailure)
        {
            return Result.Failure<Guid>(tenantResult.Error);
        }

        if (command.Email is not null)
        {
            // Handle initial admin user creation if needed
        }

        await _tenantRepository.AddAsync(tenantResult.Value, cancellationToken);

        return Result.Success(tenantResult.Value.Id);
    }
}