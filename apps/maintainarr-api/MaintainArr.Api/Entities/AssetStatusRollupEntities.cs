using STLCompliance.Shared.Data;

namespace MaintainArr.Api.Entities;

public sealed class TenantAssetStatusRollupSettings : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public bool IsEnabled { get; set; }

    public int StalenessHours { get; set; } = AssetStatusRollupDefaults.StalenessHours;

    public Guid? UpdatedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class AssetStatusRollup : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid AssetId { get; set; }

    public string AssetTag { get; set; } = string.Empty;

    public string AssetName { get; set; } = string.Empty;

    public string LifecycleStatus { get; set; } = string.Empty;

    public string ReadinessStatus { get; set; } = string.Empty;

    public string ReadinessBasis { get; set; } = string.Empty;

    public int BlockerCount { get; set; }

    public string? PrimaryBlockerMessage { get; set; }

    public int OpenCriticalDefectCount { get; set; }

    public int OpenHighDefectCount { get; set; }

    public int ActiveWorkOrderCount { get; set; }

    public int PmDueCount { get; set; }

    public int PmOverdueCount { get; set; }

    public int FailedInspectionCount { get; set; }

    public DateTimeOffset ComputedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class AssetStatusScopeRollup : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string ScopeType { get; set; } = string.Empty;

    public Guid ScopeEntityId { get; set; }

    public string? ScopeEntityKey { get; set; }

    public string ScopeLabel { get; set; } = string.Empty;

    public int TotalAssets { get; set; }

    public int ReadyCount { get; set; }

    public int NotReadyCount { get; set; }

    public decimal ReadyPercent { get; set; }

    public DateTimeOffset ComputedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class AssetStatusRollupRun : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public DateTimeOffset AsOfUtc { get; set; }

    public int CandidatesFound { get; set; }

    public int RefreshedCount { get; set; }

    public int SkippedCount { get; set; }

    public int ScopeRollupsRefreshed { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}

public static class AssetStatusRollupDefaults
{
    public const int StalenessHours = 1;
}

public static class AssetStatusRollupScopeTypes
{
    public const string Fleet = "fleet";

    public const string AssetType = "asset_type";

    public const string AssetClass = "asset_class";

    public const string Site = "site";
}
