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

public sealed record PartyRegistryCatalogOptionResponse(
    string Value,
    string Label);

public sealed record PartyRegistryMetadataResponse(
    IReadOnlyList<PartyRegistryCatalogOptionResponse> ApprovalStatusOptions,
    IReadOnlyList<PartyRegistryCatalogOptionResponse> StatusOptions,
    IReadOnlyList<PartyRegistryCatalogOptionResponse> UnitKindOptions,
    IReadOnlyList<PartyRegistryCatalogOptionResponse> ServiceTypeOptions);

public sealed record PartyContactResponse(
    Guid ContactId,
    string ContactName,
    string Email,
    string Phone,
    string RoleLabel,
    bool IsPrimary,
    DateTimeOffset CreatedAt);

public sealed record ExternalPartyResponse(
    Guid PartyId,
    string PartyKey,
    string PartyType,
    Guid? ParentPartyId,
    string? ParentPartyDisplayName,
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
    IReadOnlyList<PartyContactResponse> Contacts,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

[method: JsonConstructor]
public sealed record CreateExternalPartyRequest(
    string PartyKey,
    string PartyType,
    Guid? ParentPartyId,
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
    string? CountryCode)
{
    public CreateExternalPartyRequest(
        string partyKey,
        string partyType,
        string displayName,
        string legalName,
        string? taxIdentifier,
        string notes)
        : this(
            partyKey,
            partyType,
            null,
            null,
            displayName,
            legalName,
            taxIdentifier,
            notes,
            null,
            null,
            null,
            null,
            null,
            null,
            null)
    {
    }
}

[method: JsonConstructor]
public sealed record CreateTypedExternalPartyRequest(
    string PartyKey,
    Guid? ParentPartyId,
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
    string? CountryCode)
{
    public CreateTypedExternalPartyRequest(
        string partyKey,
        string displayName,
        string legalName,
        string? taxIdentifier,
        string notes)
        : this(
            partyKey,
            null,
            null,
            displayName,
            legalName,
            taxIdentifier,
            notes,
            null,
            null,
            null,
            null,
            null,
            null,
            null)
    {
    }
}

[method: JsonConstructor]
public sealed record UpdateExternalPartyRequest(
    Guid? ParentPartyId,
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
    string? CountryCode)
{
    public UpdateExternalPartyRequest(
        string displayName,
        string legalName,
        string? taxIdentifier,
        string notes)
        : this(
            null,
            null,
            displayName,
            legalName,
            taxIdentifier,
            notes,
            null,
            null,
            null,
            null,
            null,
            null,
            null)
    {
    }
}

public sealed record UpdateExternalPartyApprovalStatusRequest(string ApprovalStatus);

public sealed record UpdateExternalPartyStatusRequest(string Status);

public sealed record CreatePartyContactRequest(
    string ContactName,
    string Email,
    string Phone,
    string RoleLabel,
    bool IsPrimary);

public sealed record CreateExternalPartyContactRequest(
    Guid PartyId,
    string ContactName,
    string Email,
    string Phone,
    string RoleLabel,
    bool IsPrimary);
