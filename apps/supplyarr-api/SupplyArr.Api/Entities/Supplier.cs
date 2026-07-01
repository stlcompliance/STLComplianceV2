using STLCompliance.Shared.Data;

namespace SupplyArr.Api.Entities;

public sealed class Supplier : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string SupplierKey { get; set; } = string.Empty;

    public Guid? ParentSupplierId { get; set; }

    public string UnitKind { get; set; } = "identity";

    public string DisplayName { get; set; } = string.Empty;

    public string LegalName { get; set; } = string.Empty;

    public string? TaxIdentifier { get; set; }

    public string ApprovalStatus { get; set; } = "pending";

    public string Status { get; set; } = "active";

    public string Notes { get; set; } = string.Empty;

    public string ServiceTypesJson { get; set; } = "[]";

    public string AddressLine1 { get; set; } = string.Empty;

    public string AddressLine2 { get; set; } = string.Empty;

    public string Locality { get; set; } = string.Empty;

    public string RegionCode { get; set; } = string.Empty;

    public string PostalCode { get; set; } = string.Empty;

    public string CountryCode { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public Supplier? ParentSupplier { get; set; }

    public ICollection<SupplierContact> Contacts { get; set; } = new List<SupplierContact>();

    public ICollection<Supplier> ChildSuppliers { get; set; } = new List<Supplier>();
}

