using MaintainArr.Api.Contracts;
using MaintainArr.Api.Entities;

namespace MaintainArr.Api.Services;

public static class MaintenanceHistoryRules
{
    public const int DefaultReadStalenessHours = MaintenanceHistoryRollupDefaults.StalenessHours;

    public static int NormalizeBatchSize(int? batchSize) =>
        Math.Clamp(batchSize ?? 50, 1, 500);

    public static int NormalizeStalenessHours(int? stalenessHours) =>
        Math.Clamp(stalenessHours ?? MaintenanceHistoryRollupDefaults.StalenessHours, 1, 168);

    public static int NormalizeRunListLimit(int? limit) =>
        Math.Clamp(limit ?? 10, 1, 100);

    public static bool IsStale(DateTimeOffset? computedAt, DateTimeOffset asOfUtc, int stalenessHours)
    {
        if (computedAt is null)
        {
            return true;
        }

        var threshold = asOfUtc.AddHours(-stalenessHours);
        return computedAt < threshold;
    }

    public static MaintenanceHistoryCategoryCounts AggregateCategoryCounts(
        IReadOnlyList<MaintenanceHistoryEntryResponse> entries)
    {
        var inspection = 0;
        var defect = 0;
        var workOrder = 0;
        var pm = 0;

        foreach (var entry in entries)
        {
            switch (entry.Category)
            {
                case "inspection":
                    inspection++;
                    break;
                case "defect":
                    defect++;
                    break;
                case "work_order":
                    workOrder++;
                    break;
                case "pm":
                    pm++;
                    break;
            }
        }

        return new MaintenanceHistoryCategoryCounts(inspection, defect, workOrder, pm);
    }
}

public sealed record MaintenanceHistoryCategoryCounts(
    int InspectionCount,
    int DefectCount,
    int WorkOrderCount,
    int PmCount);
