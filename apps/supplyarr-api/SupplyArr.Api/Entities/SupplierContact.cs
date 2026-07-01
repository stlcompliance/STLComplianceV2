using STLCompliance.Shared.Data;

namespace SupplyArr.Api.Entities;

public sealed class SupplierContact : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid SupplierId { get; set; }

    public string ContactName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string RoleLabel { get; set; } = string.Empty;

    public bool IsPrimary { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public Supplier? Supplier { get; set; }
}

