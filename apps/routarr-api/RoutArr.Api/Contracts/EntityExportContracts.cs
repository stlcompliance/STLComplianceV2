namespace RoutArr.Api.Contracts;

public sealed record EntityExportFormatDescriptor(
    string FormatKey,
    string ContentType,
    string FileNamePattern,
    string Description);

public sealed record EntityExportDescriptor(
    string EntityKey,
    string Route,
    string Label,
    string CsvHeader,
    string Description,
    IReadOnlyList<EntityExportFormatDescriptor> Formats);

public sealed record ReportExportDescriptor(
    string ReportKey,
    string Route,
    string Label,
    string Description);

public sealed record EntityExportManifestResponse(
    string PackageVersion,
    IReadOnlyList<EntityExportDescriptor> Entities,
    IReadOnlyList<ReportExportDescriptor> ReportExports,
    IReadOnlyList<EntityExportFormatDescriptor> AuditPackageFormats);
