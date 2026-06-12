namespace SupplyArr.Api.Contracts;

public sealed record PartyRegistryCatalogOptionResponse(
    string Value,
    string Label);

public sealed record PartyRegistryMetadataResponse(
    IReadOnlyList<PartyRegistryCatalogOptionResponse> ApprovalStatusOptions,
    IReadOnlyList<PartyRegistryCatalogOptionResponse> StatusOptions);

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
    string DisplayName,
    string LegalName,
    string? TaxIdentifier,
    string ApprovalStatus,
    string Status,
    string Notes,
    IReadOnlyList<PartyContactResponse> Contacts,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateExternalPartyRequest(
    string PartyKey,
    string PartyType,
    string DisplayName,
    string LegalName,
    string? TaxIdentifier,
    string Notes);

public sealed record CreateTypedExternalPartyRequest(
    string PartyKey,
    string DisplayName,
    string LegalName,
    string? TaxIdentifier,
    string Notes);

public sealed record UpdateExternalPartyRequest(
    string DisplayName,
    string LegalName,
    string? TaxIdentifier,
    string Notes);

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
