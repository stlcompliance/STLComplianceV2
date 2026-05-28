using STLCompliance.Shared.Data;

namespace SupplyArr.Api.Entities;

public sealed class TenantDemandProcessingSettings : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public bool IsEnabled { get; set; }

    public bool AutoCreatePrDraftWhenShort { get; set; }

    public int MinHoursBeforeProcessing { get; set; } = DemandProcessingDefaults.MinHoursBeforeProcessing;

    public int StalenessHours { get; set; } = DemandProcessingDefaults.StalenessHours;

    public bool NotifyOnPrDraftCreated { get; set; } = true;

    public bool ProcessMaintainarrDemandRefs { get; set; } = true;

    public bool ProcessRoutarrDemandRefs { get; set; }

    public bool ProcessTrainarrDemandRefs { get; set; }

    public bool ProcessStaffarrDemandRefs { get; set; }

    public Guid? UpdatedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class DemandProcessingState : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid DemandRefId { get; set; }

    public string DemandRefSource { get; set; } = DemandRefSources.MaintainArr;

    public string MaintainarrWorkOrderNumber { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string ProcessingOutcome { get; set; } = string.Empty;

    public string RecommendedAction { get; set; } = string.Empty;

    public int LinesTotalCount { get; set; }

    public int LinesCatalogCount { get; set; }

    public int LinesShortCount { get; set; }

    public Guid? PurchaseRequestId { get; set; }

    public string? LastProcessingMessage { get; set; }

    public DateTimeOffset DemandReceivedAt { get; set; }

    public DateTimeOffset LastProcessedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public static class DemandRefSources
{
    public const string MaintainArr = "maintainarr";

    public const string RoutArr = "routarr";

    public const string TrainArr = "trainarr";

    public const string StaffArr = "staffarr";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        MaintainArr,
        RoutArr,
        TrainArr,
        StaffArr,
    };
}

public sealed class DemandProcessingRun : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public DateTimeOffset AsOfUtc { get; set; }

    public int CandidatesFound { get; set; }

    public int ProcessedCount { get; set; }

    public int PrDraftsCreatedCount { get; set; }

    public int SkippedCount { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}

public static class DemandProcessingDefaults
{
    public const int MinHoursBeforeProcessing = 0;

    public const int StalenessHours = 4;
}

public static class DemandProcessingOutcomes
{
    public const string StockAvailable = "stock_available";

    public const string StockShort = "stock_short";

    public const string NoCatalogParts = "no_catalog_parts";

    public const string PrDrafted = "pr_drafted";

    public const string PartialCatalog = "partial_catalog";
}

public static class DemandProcessingRecommendedActions
{
    public const string FulfillFromStock = "fulfill_from_stock";

    public const string CreatePurchaseRequest = "create_purchase_request";

    public const string ReviewManually = "review_manually";

    public const string PrAutoCreated = "pr_auto_created";
}
