namespace ComplianceCore.Api.Contracts;

public sealed record CsvBundleManifestResponse(
    IReadOnlyList<CsvBundleFileDescriptor> Files);

public sealed record CsvBundleFileDescriptor(
    string FileName,
    IReadOnlyList<string> Headers);

public sealed record CsvImportResultResponse(
    bool DryRun,
    bool Applied,
    IReadOnlyList<CsvImportFileSummary> Files,
    IReadOnlyList<CsvImportIssue> Issues);

public sealed record CsvImportFileSummary(
    string FileName,
    int RowCount,
    int Created,
    int Updated,
    int Deactivated);

public sealed record CsvImportIssue(
    string FileName,
    int? LineNumber,
    string Code,
    string Message);

public sealed record RulePackImportRunResponse(
    Guid ImportId,
    string Status,
    bool DryRun,
    DateTimeOffset CreatedAt,
    CsvImportResultResponse Result);

public sealed record RulePackImportDiffResponse(
    Guid ImportId,
    int FilesWithChanges,
    int CreatedCount,
    int UpdatedCount,
    int DeactivatedCount,
    int IssueCount);

public sealed record RulePackImportTestResultsResponse(
    Guid ImportId,
    bool Passed,
    int IssueCount,
    IReadOnlyList<CsvImportIssue> Issues);

public sealed record RulePackImportRollbackResponse(
    Guid ImportId,
    bool RolledBack,
    string Status);
