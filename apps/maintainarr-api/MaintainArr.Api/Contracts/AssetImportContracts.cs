namespace MaintainArr.Api.Contracts;

public sealed record AssetImportRowRequest
{
    public string AssetTag { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string LifecycleStatus { get; init; } = "in_service";
    public IReadOnlyDictionary<string, string?> Values { get; init; } = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

    public string AssetClassKey { get; init; } = string.Empty;
    public string AssetTypeKey { get; init; } = string.Empty;
    public string? SiteRef { get; init; }

    public AssetImportRowRequest()
    {
    }

    public AssetImportRowRequest(
        string assetClassKey,
        string assetTypeKey,
        string assetTag,
        string name,
        string description = "",
        string? siteRef = null,
        string lifecycleStatus = "active")
    {
        AssetTag = assetTag;
        Name = name;
        Description = description;
        LifecycleStatus = lifecycleStatus;
        AssetClassKey = assetClassKey;
        AssetTypeKey = assetTypeKey;
        SiteRef = siteRef;
        Values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["assetClass"] = assetClassKey,
            ["assetType"] = assetTypeKey,
            ["siteId"] = siteRef,
            ["lifecycleStatus"] = lifecycleStatus,
        };
    }

    public AssetImportRowRequest(
        string assetTag,
        string name,
        IReadOnlyDictionary<string, string?> values,
        string description = "",
        string lifecycleStatus = "in_service")
    {
        AssetTag = assetTag;
        Name = name;
        Description = description;
        LifecycleStatus = lifecycleStatus;
        Values = values;
    }
}

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
