namespace StaffArr.Api.Contracts;

public sealed record PersonExportManifestResponse(
    string PackageVersion,
    string CsvHeader,
    IReadOnlyList<PersonExportFormatDescriptor> Formats);

public sealed record PersonExportFormatDescriptor(
    string Key,
    string ContentType,
    string FileName,
    string Description);

public sealed record PersonExportRowItem(
    Guid PersonId,
    string GivenName,
    string FamilyName,
    string PrimaryEmail,
    string EmploymentStatus,
    string? JobTitle,
    string? ManagerEmail,
    Guid? PrimaryOrgUnitId,
    string? PrimaryOrgUnitName,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record PersonExportResponse(
    Guid ExportId,
    Guid TenantId,
    DateTimeOffset GeneratedAt,
    int PersonCount,
    IReadOnlyList<PersonExportRowItem> People);

public sealed record PersonExportFilterRequest(
    string? EmploymentStatus,
    Guid? OrgUnitId);

public sealed record PersonExportPresetResponse(
    string? EmploymentStatus,
    Guid? OrgUnitId,
    string? PresetKey,
    DateTimeOffset UpdatedAt);

public sealed record UpsertPersonExportPresetRequest(
    string? EmploymentStatus,
    Guid? OrgUnitId,
    string? PresetKey);
