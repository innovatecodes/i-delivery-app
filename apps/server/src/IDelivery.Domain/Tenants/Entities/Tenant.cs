// Bounded Context: Tenants (Multi-Tenancy)
// Contexto delimitado responsável pela gestão de tenants (empresas/restaurantes) no SaaS.
// Contém: Entities, ValueObjects, Enums, Events, Repositories específicos deste contexto.

using IDelivery.Domain.Common.DomainEvents;
using IDelivery.Domain.Common.Result;
using IDelivery.Domain.Entities;
using IDelivery.Domain.Tenants.ValueObjects;
using IDelivery.Domain.Tenants.Enums;
using IDelivery.Domain.Tenants.Events;

namespace IDelivery.Domain.Tenants.Entities;

/// <summary>
/// Aggregate Root do Tenant.
/// Representa uma empresa/restaurante/estabelecimento que utiliza o SaaS.
/// Responsável por manter a consistência das regras de negócio do tenant.
/// </summary>
public sealed class Tenant : AggregateRoot
{
    public string Name { get; private set; } = null!;
    public string Slug { get; private set; } = null!;
    public string? Description { get; private set; }
    public TenantStatus Status { get; private set; }
    public string? LogoUrl { get; private set; }
    public Address? Address { get; private set; }
    public ContactInfo? ContactInfo { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Tenant() { }

    private Tenant(
        Guid id,
        string name,
        string slug,
        string? description,
        string? logoUrl,
        Address? address,
        ContactInfo? contactInfo) : base(id)
    {
        Name = name;
        Slug = slug;
        Description = description;
        LogoUrl = logoUrl;
        Address = address;
        ContactInfo = contactInfo;
        Status = TenantStatus.Active;
        CreatedAt = DateTime.UtcNow;

        AddDomainEvent(new TenantCreatedDomainEvent(id, name, slug));
    }

    /// <summary>
    /// Factory method para criar um novo Tenant com validações de negócio.
    /// </summary>
    public static Result<Tenant> Create(
        string name,
        string slug,
        string? description = null,
        string? logoUrl = null,
        Address? address = null,
        ContactInfo? contactInfo = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Tenant>(new Error("Tenant.NameRequired", "Nome do tenant é obrigatório"));

        if (string.IsNullOrWhiteSpace(slug))
            return Result.Failure<Tenant>(new Error("Tenant.SlugRequired", "Slug do tenant é obrigatório"));

        if (name.Length > 200)
            return Result.Failure<Tenant>(new Error("Tenant.NameTooLong", "Nome do tenant deve ter no máximo 200 caracteres"));

        if (slug.Length > 100)
            return Result.Failure<Tenant>(new Error("Tenant.SlugTooLong", "Slug do tenant deve ter no máximo 100 caracteres"));

        var tenant = new Tenant(Guid.NewGuid(), name.Trim(), slug.Trim().ToLowerInvariant(), description, logoUrl, address, contactInfo);
        return Result.Success(tenant);
    }

    public Result UpdateDetails(string name, string? description, string? logoUrl)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(new Error("Tenant.NameRequired", "Nome do tenant é obrigatório"));

        if (name.Length > 200)
            return Result.Failure(new Error("Tenant.NameTooLong", "Nome do tenant deve ter no máximo 200 caracteres"));

        Name = name.Trim();
        Description = description;
        LogoUrl = logoUrl;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new TenantUpdatedDomainEvent(Id, Name));

        return Result.Success();
    }

    public Result UpdateAddress(Address address)
    {
        Address = address;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result UpdateContactInfo(ContactInfo contactInfo)
    {
        ContactInfo = contactInfo;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result Activate()
    {
        if (Status == TenantStatus.Active)
            return Result.Failure(new Error("Tenant.AlreadyActive", "Tenant já está ativo"));

        Status = TenantStatus.Active;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new TenantActivatedDomainEvent(Id));
        return Result.Success();
    }

    public Result Block()
    {
        if (Status == TenantStatus.Blocked)
            return Result.Failure(new Error("Tenant.AlreadyBlocked", "Tenant já está bloqueado"));

        Status = TenantStatus.Blocked;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new TenantBlockedDomainEvent(Id));
        return Result.Success();
    }
}