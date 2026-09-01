namespace IDelivery.Application.Queries.Customers;

public sealed record CustomerResponse(
    Guid Id,
    Guid TenantId,
    Guid UserId,
    string FullName,
    string Email,
    string? PhoneNumber,
    string? Notes,
    bool IsActive,
    List<CustomerAddressResponse> Addresses,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record CustomerAddressResponse(
    Guid Id,
    string Label,
    string Street,
    string Number,
    string? Complement,
    string Neighborhood,
    string City,
    string State,
    string ZipCode,
    string? Reference,
    bool IsDefault,
    DateTime CreatedAt);
