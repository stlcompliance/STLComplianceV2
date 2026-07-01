using System.Text.Json.Serialization;

namespace SupplyArr.Api.Contracts;

public sealed record SupplierDirectoryCatalogOptionResponse(
    string Value,
    string Label);

public sealed record SupplierDirectoryMetadataResponse(
    IReadOnlyList<SupplierDirectoryCatalogOptionResponse> ApprovalStatusOptions,
    IReadOnlyList<SupplierDirectoryCatalogOptionResponse> StatusOptions,
    IReadOnlyList<SupplierDirectoryCatalogOptionResponse> UnitKindOptions,
    IReadOnlyList<SupplierDirectoryCatalogOptionResponse> ServiceTypeOptions);

public sealed record SupplierContactResponse(
    Guid ContactId,
    string ContactName,
    string Email,
    string Phone,
    string RoleLabel,
    bool IsPrimary,
    DateTimeOffset CreatedAt);

public sealed record SupplierResponse(
    Guid SupplierId,
    string SupplierKey,
    Guid? ParentSupplierId,
    string? ParentSupplierDisplayName,
    string UnitKind,
    string DisplayName,
    string LegalName,
    string? TaxIdentifier,
    string ApprovalStatus,
    string Status,
    string Notes,
    IReadOnlyList<string> ServiceTypes,
    string AddressLine1,
    string AddressLine2,
    string Locality,
    string RegionCode,
    string PostalCode,
    string CountryCode,
    int ChildUnitCount,
    IReadOnlyList<SupplierContactResponse> Contacts,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateSupplierRequest(
    string SupplierKey,
    Guid? ParentSupplierId,
    string? UnitKind,
    string DisplayName,
    string LegalName,
    string? TaxIdentifier,
    string Notes,
    IReadOnlyList<string>? ServiceTypes,
    string? AddressLine1,
    string? AddressLine2,
    string? Locality,
    string? RegionCode,
    string? PostalCode,
    string? CountryCode);

public sealed record UpdateSupplierRequest(
    Guid? ParentSupplierId,
    string? UnitKind,
    string DisplayName,
    string LegalName,
    string? TaxIdentifier,
    string Notes,
    IReadOnlyList<string>? ServiceTypes,
    string? AddressLine1,
    string? AddressLine2,
    string? Locality,
    string? RegionCode,
    string? PostalCode,
    string? CountryCode);

public sealed record UpdateSupplierApprovalStatusRequest(string ApprovalStatus);

public sealed record UpdateSupplierStatusRequest(string Status);

public sealed record CreateSupplierContactRequest(
    string ContactName,
    string Email,
    string Phone,
    string RoleLabel,
    bool IsPrimary);
