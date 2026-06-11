namespace StaffArr.Api.Contracts;

public sealed record EntityExportFormatDescriptor(
    string FormatKey,
    string ContentType,
    string FileNameTemplate,
    string Description);

public sealed record EntityExportManifestEntity(
    string EntityKey,
    string ExportPath,
    string DisplayName,
    string CsvHeader,
    string Description,
    IReadOnlyList<EntityExportFormatDescriptor> Formats);

public sealed record EntityExportManifestResponse(
    string PackageVersion,
    IReadOnlyList<EntityExportManifestEntity> Entities);

public sealed record CsvExportResult(string ContentType, string FileName, byte[] Content);
