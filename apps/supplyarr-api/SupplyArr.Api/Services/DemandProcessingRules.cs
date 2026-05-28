using SupplyArr.Api.Contracts;
using SupplyArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace SupplyArr.Api.Services;

public static class DemandProcessingRules
{
    public static void ValidateSettings(UpsertDemandProcessingSettingsRequest request)
    {
        var anySource = request.ProcessMaintainarrDemandRefs
            || request.ProcessRoutarrDemandRefs
            || request.ProcessTrainarrDemandRefs
            || request.ProcessStaffarrDemandRefs;

        if (request.IsEnabled && !anySource)
        {
            throw new StlApiException(
                "demand_processing_settings.no_sources",
                "Enable at least one demand source when the demand processing worker is on.",
                400);
        }

        if (request.AutoCreatePrDraftWhenShort && !anySource)
        {
            throw new StlApiException(
                "demand_processing_settings.auto_pr_requires_source",
                "Auto PR draft requires at least one enabled demand source.",
                400);
        }
    }

    public static int NormalizeBatchSize(int? batchSize) =>
        Math.Clamp(batchSize ?? 50, 1, 500);

    public static int NormalizeMinHours(int? hours) =>
        Math.Clamp(hours ?? DemandProcessingDefaults.MinHoursBeforeProcessing, 0, 168);

    public static int NormalizeStalenessHours(int? hours) =>
        Math.Clamp(hours ?? DemandProcessingDefaults.StalenessHours, 1, 168);

    public static int NormalizeRunListLimit(int? limit) =>
        Math.Clamp(limit ?? 10, 1, 100);

    public static bool IsDueForProcessing(
        DateTimeOffset receivedAt,
        DateTimeOffset? lastProcessedAt,
        int minHoursBeforeProcessing,
        int stalenessHours,
        DateTimeOffset asOfUtc)
    {
        if (asOfUtc < receivedAt.AddHours(minHoursBeforeProcessing))
        {
            return false;
        }

        if (lastProcessedAt is null)
        {
            return true;
        }

        return asOfUtc >= lastProcessedAt.Value.AddHours(stalenessHours);
    }

    public static (string Outcome, string RecommendedAction) ResolveOutcome(
        int linesTotalCount,
        int linesCatalogCount,
        int linesShortCount)
    {
        if (linesCatalogCount == 0)
        {
            return (DemandProcessingOutcomes.NoCatalogParts, DemandProcessingRecommendedActions.ReviewManually);
        }

        if (linesShortCount > 0)
        {
            return (DemandProcessingOutcomes.StockShort, DemandProcessingRecommendedActions.CreatePurchaseRequest);
        }

        if (linesCatalogCount < linesTotalCount)
        {
            return (DemandProcessingOutcomes.PartialCatalog, DemandProcessingRecommendedActions.ReviewManually);
        }

        return (DemandProcessingOutcomes.StockAvailable, DemandProcessingRecommendedActions.FulfillFromStock);
    }

    public static string BuildProcessingMessage(
        string outcome,
        int linesShortCount,
        int linesCatalogCount,
        int linesTotalCount) =>
        outcome switch
        {
            DemandProcessingOutcomes.StockAvailable =>
                $"All {linesCatalogCount} catalog-linked lines have sufficient stock.",
            DemandProcessingOutcomes.StockShort =>
                $"{linesShortCount} of {linesCatalogCount} catalog-linked lines are short on stock.",
            DemandProcessingOutcomes.NoCatalogParts =>
                "No catalog-linked parts on demand reference.",
            DemandProcessingOutcomes.PartialCatalog =>
                $"{linesCatalogCount} of {linesTotalCount} lines are catalog-linked; remaining lines need manual review.",
            DemandProcessingOutcomes.PrDrafted =>
                "Purchase request draft auto-created due to stock shortage.",
            _ => "Demand processing completed.",
        };
}

public sealed record TenantDemandProcessingSettingsSnapshot(
    bool IsEnabled,
    bool AutoCreatePrDraftWhenShort,
    int MinHoursBeforeProcessing,
    int StalenessHours,
    bool NotifyOnPrDraftCreated,
    bool ProcessMaintainarrDemandRefs,
    bool ProcessRoutarrDemandRefs,
    bool ProcessTrainarrDemandRefs,
    bool ProcessStaffarrDemandRefs)
{
    public bool IsSourceEnabled(string demandRefSource) =>
        demandRefSource switch
        {
            DemandRefSources.MaintainArr => ProcessMaintainarrDemandRefs,
            DemandRefSources.RoutArr => ProcessRoutarrDemandRefs,
            DemandRefSources.TrainArr => ProcessTrainarrDemandRefs,
            DemandRefSources.StaffArr => ProcessStaffarrDemandRefs,
            _ => false,
        };
}
