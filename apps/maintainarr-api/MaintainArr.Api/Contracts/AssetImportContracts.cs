namespace MaintainArr.Api.Contracts;

public sealed record AssetImportRowRequest(
    string AssetClassKey,
    string AssetTypeKey,
    string AssetTag,
    string Name,
    string Description = "",
    string? SiteRef = null,
    string LifecycleStatus = "active");

public sealed record AssetBulkImportRequest(
    IReadOnlyList<AssetImportRowRequest> Assets);

public sealed record AssetImportRowResult(
    int RowIndex,
    string AssetTag,
    string Status,
    Guid? AssetId,
    string? ErrorCode,
    string? Message);

public sealed record AssetBulkImportResponse(
    Guid ImportBatchId,
    string ImportType,
    string Phase,
    bool DryRun,
    int TotalRows,
    int SuccessCount,
    int ErrorCount,
    IReadOnlyList<AssetImportRowResult> Results);
