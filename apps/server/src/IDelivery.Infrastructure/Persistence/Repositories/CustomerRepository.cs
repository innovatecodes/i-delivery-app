using Microsoft.EntityFrameworkCore;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.Domain.Customers.Entities;
using IDelivery.Infrastructure.Persistence.Context;

namespace IDelivery.Infrastructure.Persistence.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly ApplicationDbContext _context;

    public CustomerRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Customers
            .Include(c => c.Addresses)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Customer?> GetByUserIdAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Customers
            .Include(c => c.Addresses)
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.UserId == userId, cancellationToken);
    }

    public async Task<Customer?> GetByEmailAsync(Guid tenantId, string email, CancellationToken cancellationToken = default)
    {
        return await _context.Customers
            .Include(c => c.Addresses)
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Email == email, cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(Guid tenantId, string email, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        return await _context.Customers
            .AnyAsync(c => c.TenantId == tenantId && c.Email == email && (excludeId == null || c.Id != excludeId), cancellationToken);
    }

    public async Task AddAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        await _context.Customers.AddAsync(customer, cancellationToken);
    }

    public async Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        _context.Customers.Update(customer);
    }

    public async Task DeleteAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        _context.Customers.Remove(customer);
    }
}
